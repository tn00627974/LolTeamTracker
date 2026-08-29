/*
    索引體檢：頁數、每列大小、頁面使用率、碎片率

    用途：
      1. 判斷索引該不該重建（看碎片率與頁面使用率，不要憑感覺）
      2. 對照「涵蓋索引」與「回主表」的成本差距（看每列位元組與每頁列數）
      3. 觀察 B-Tree 的層級結構（層級 0 是葉層，資料真正存放處）

    ⚠️ 最後一個參數 'DETAILED' 會掃描整個索引的每一頁。
       本專案 25 萬列跑起來是瞬間，但正式環境的大表要改用 'SAMPLED'（抽樣 1%）
       或 'LIMITED'（只讀上層，最快，但拿不到 avg_record_size_in_bytes）。

    判讀重點：
      - 只看「層級 0」那幾行。上層節點頁面使用率天生偏低（50%、3%），那是正常的
      - 碎片率 < 5% 不用管；5–30% REORGANIZE；> 30% REBUILD
      - 但小於 1000 頁的索引，碎片率再高都不用處理——它整個塞得進記憶體
      - SSD 時代，頁面使用率其實比碎片率更值得看：頁面半空代表同樣資料
        要用兩倍的頁來裝，直接吃掉 buffer pool

    2026-08-28 實測（MatchPlayers 249,117 列，剛重建無碎片）：
      PK_MatchPlayers                    3,727 頁   118.4 bytes/列   ← 叢集索引存整列
      IX_MatchPlayers_PlayerId_GameDate  1,783 頁    55.8 bytes/列   ← 帶 INCLUDE(MatchId)
      IX_MatchPlayers_MatchId_PlayerId   1,538 頁    47.8 bytes/列

    這組數字解釋了「95% 成本在 Key Lookup」的成因：
    主表每列 118.4 bytes 是索引的兩倍多，一頁只裝得下約 68 列（索引可裝 140 列）。
    每次 Key Lookup 都要去讀那個又寬又大的主表頁面。
*/

USE LolTeamTracker;

DECLARE @TableName sysname = 'MatchPlayers';   -- 改這裡看別張表

SELECT
    i.name                                                   AS 索引名稱,
    ps.index_level                                           AS 層級,
    ps.page_count                                            AS 頁數,
    ps.record_count                                          AS 列數,
    CAST(ps.avg_record_size_in_bytes AS decimal(10,1))       AS 每列位元組,
    CAST(ps.avg_page_space_used_in_percent AS decimal(5,1))  AS 頁面使用率,
    CAST(ps.avg_fragmentation_in_percent AS decimal(5,1))    AS 碎片率,

    -- 實際：資料庫真的把幾列塞進一頁（record_count ÷ page_count）
    CAST(ps.record_count * 1.0 / NULLIF(ps.page_count, 0) AS decimal(10,1))
        AS 每頁列數_實際,

    -- 理論：(8096 bytes 可用空間 × 使用率) ÷ 每列大小
    -- 一頁 8192 bytes 扣掉 96 bytes 頁標頭 = 8096 可放資料
    -- 理論值會略高於實際值，因為沒扣掉每列 2 bytes 的位置指標與頁尾零頭
    CAST(8096.0 * (ps.avg_page_space_used_in_percent / 100.0)
         / NULLIF(ps.avg_record_size_in_bytes, 0) AS decimal(10,1))
        AS 每頁列數_理論
FROM sys.dm_db_index_physical_stats(
        DB_ID(),
        OBJECT_ID(@TableName),
        NULL,           -- 索引 ID，NULL = 全部索引
        NULL,
        'DETAILED') ps
JOIN sys.indexes i
     ON ps.object_id = i.object_id
    AND ps.index_id  = i.index_id
WHERE i.name IS NOT NULL
ORDER BY i.name, ps.index_level;


/*
    索引欄位明細：確認 INCLUDE 有沒有真的生效

    is_included_column = 0 → 索引鍵欄位（key_ordinal 是它的順序）
    is_included_column = 1 → INCLUDE 欄位（不參與排序，只是被帶著走）
*/
SELECT
    i.name                  AS 索引名稱,
    c.name                  AS 欄位,
    ic.key_ordinal          AS 鍵順序,
    ic.is_included_column   AS 是否為INCLUDE,
    ic.is_descending_key    AS 是否降冪
FROM sys.indexes i
JOIN sys.index_columns ic
     ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c
     ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID(@TableName)
  AND i.name IS NOT NULL
ORDER BY i.name, ic.is_included_column, ic.key_ordinal;
