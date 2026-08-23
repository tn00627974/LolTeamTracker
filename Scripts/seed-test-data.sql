/*
    開發環境測試資料：比賽資料（Matches / MatchPlayers / QueueDefinitions）

    用途：驗證索引效果需要「夠大、查起來會慢」的資料量。
          2026-08-22 用這份資料量出索引的實際效益：
            無索引        3,798 次邏輯讀取 / 34 ms
            有索引        1,051 次（95% 成本在 Key Lookup）
            涵蓋索引        151 次（執行計畫只剩一個 Index Seek）

    ⚠️ 僅供開發環境使用。腳本會 TRUNCATE / DELETE 既有的比賽資料。
       正式環境的資料由應用程式的匯入流程寫入，絕不執行本腳本。

    執行方式（任選一種）：
      1. SSMS 連線至 localhost,1434 後開啟本檔案執行
      2. docker exec lolteamtracker-mssql /opt/mssql-tools18/bin/sqlcmd \
             -S localhost -U sa -P "你的密碼" -C -d LolTeamTracker -i seed-test-data.sql

    前置條件：
      1. 已執行 dotnet ef database update 建立四張資料表
      2. 已執行 seed-players.sql 建立戰隊成員（MatchPlayers 需要 PlayerId 外鍵）
      3. 已執行 seed-queue-definitions.sql 建立模式對照表（Matches 需要 QueueId 外鍵）

    本腳本可重複執行：QueueDefinitions 以 Id 判斷是否存在；
    比賽資料則先清空再重建（因為 MatchId 是固定產生的 TW2_1 ~ TW2_100000）。
*/

USE LolTeamTracker;
GO

-- ══════════════════════════════════════════════════════════════
-- 安全檢查：防止在錯誤的資料庫執行
-- ══════════════════════════════════════════════════════════════
-- 應用層的檢查擋不住蓄意，但擋得住手滑——而手滑才是真正會發生的事。
IF DB_NAME() <> 'LolTeamTracker'
BEGIN
    RAISERROR('本腳本只能在 LolTeamTracker 開發資料庫執行，目前連線的是 %s', 16, 1, @@SERVERNAME);
    RETURN;
END
GO

DECLARE @MatchCount int = 100000;   -- 要生成幾場比賽（MatchPlayers 約為此數的 2.5 倍）

-- ══════════════════════════════════════════════════════════════
-- ① 清空既有比賽資料
-- ══════════════════════════════════════════════════════════════
-- 順序必須與外鍵相反：先刪子表（MatchPlayers）才能刪父表（Matches）。
--
-- MatchPlayers 用 TRUNCATE：沒有任何表參考它，而且會一併重設 identity。
-- Matches 只能用 DELETE：它被 MatchPlayers 的外鍵參考，
--   即使子表已清空，SQL Server 仍禁止 TRUNCATE 被參考的表。
TRUNCATE TABLE MatchPlayers;
DELETE FROM Matches;

-- ══════════════════════════════════════════════════════════════
-- ② Matches：一場比賽本身的資料
-- ══════════════════════════════════════════════════════════════
-- 產生大量列的技巧：SQL Server 沒有 generate_series，
-- 所以拿系統表 CROSS JOIN 自己當基底（2000+ 列 × 2000+ 列 = 400 萬列可用）。
-- ROW_NUMBER() 的 ORDER BY (SELECT NULL) 表示「我要編號，但不在乎順序」。
;WITH N AS (
    SELECT TOP (@MatchCount)
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO Matches (Id, QueueId, GameDate, GameDuration, CreatedAt, UpdatedAt)
SELECT
    'TW2_' + CAST(n AS varchar(10)),

    -- 模式分布刻意不均勻，模擬真實情況（大亂鬥最多、排位次之）
    CASE
        WHEN n % 100 < 50 THEN 450   -- ARAM          50%
        WHEN n % 100 < 70 THEN 420   -- Solo/Duo      20%
        WHEN n % 100 < 85 THEN 440   -- Flex          15%
        WHEN n % 100 < 95 THEN 400   -- Normal Draft  10%
        ELSE                   480   -- Swiftplay      5%
    END,

    -- 開賽時間散佈在過去 180 天內。
    -- 🔑 時間必須有分布，否則 (PlayerId, GameDate) 索引的選擇性等於零，量不出效果。
    DATEADD(SECOND,
            -ABS(CHECKSUM(NEWID()) % (180 * 24 * 60 * 60)),
            SYSUTCDATETIME()),

    1200 + ABS(CHECKSUM(NEWID()) % 1201),   -- 時長 1200~2400 秒（20~40 分鐘）

    SYSUTCDATETIME(), SYSUTCDATETIME()
FROM N;

PRINT CONCAT('Matches 已寫入 ', @@ROWCOUNT, ' 筆');

-- ══════════════════════════════════════════════════════════════
-- ③ MatchPlayers：某位玩家在某場比賽的表現
-- ══════════════════════════════════════════════════════════════
-- 🔑 用 CROSS JOIN 是刻意的：它產生的每個 (MatchId, PlayerId) 組合天生只出現一次，
--    因此不可能違反 IX_MatchPlayers_MatchId_PlayerId 這個唯一索引。
--    用結構保證，不靠運氣。
INSERT INTO MatchPlayers
    (MatchId, PlayerId, ChampionId, Kills, Deaths, Assists, Win,
     TeamPosition, LaneCS, JungleCS, Gold, GameDate, CreatedAt, UpdatedAt)
SELECT
    m.Id,
    p.Id,

    -- 英雄池：讓每個玩家常玩的英雄集中在 8 隻左右。
    -- 若寫成完全隨機（% 160），每隻英雄只有幾場，
    -- RANK() 排出來的「最常玩前三名」會是雜訊，看不出函數算對沒有。
    ((p.Id * 13) + ABS(CHECKSUM(NEWID()) % 8)) % 160 + 1,

    ABS(CHECKSUM(NEWID()) % 21),     -- Kills   0~20
    ABS(CHECKSUM(NEWID()) % 16),     -- Deaths  0~15
    ABS(CHECKSUM(NEWID()) % 26),     -- Assists 0~25

    -- 勝負：讓不同玩家的勝率落在 35%~65%。
    -- 若純隨機，所有人都是 50%，「勝率最高的路線」排出來會是隨機順序。
    CASE WHEN ABS(CHECKSUM(NEWID()) % 100) < (35 + (p.Id * 7 % 30)) THEN 1 ELSE 0 END,

    CASE ABS(CHECKSUM(NEWID()) % 5)
        WHEN 0 THEN 'TOP'
        WHEN 1 THEN 'JUNGLE'
        WHEN 2 THEN 'MIDDLE'
        WHEN 3 THEN 'BOTTOM'
        ELSE        'UTILITY'
    END,

    ABS(CHECKSUM(NEWID()) % 301),            -- LaneCS   0~300
    ABS(CHECKSUM(NEWID()) % 101),            -- JungleCS 0~100
    5000 + ABS(CHECKSUM(NEWID()) % 15001),   -- Gold     5000~20000

    -- 🔑 反正規化欄位：必須從 Matches 抄過來，不能重新產生。
    -- 兩張表的 GameDate 若不一致，之後 JOIN 查詢會出現「同一場比賽兩個時間」。
    m.GameDate,

    SYSUTCDATETIME(), SYSUTCDATETIME()
FROM Matches m
CROSS JOIN Players p
-- 過濾掉約 3/4 的組合，模擬「不是每場都全員參加」。平均每場約 2~3 人。
WHERE (ABS(CHECKSUM(m.Id) % 100) + p.Id) % 4 = 0;

PRINT CONCAT('MatchPlayers 已寫入 ', @@ROWCOUNT, ' 筆');
GO

-- ══════════════════════════════════════════════════════════════
-- ④ 驗證
-- ══════════════════════════════════════════════════════════════
SELECT 'QueueDefinitions' AS TableName, COUNT(*) AS Rows FROM QueueDefinitions
UNION ALL SELECT 'Players',      COUNT(*) FROM Players
UNION ALL SELECT 'Matches',      COUNT(*) FROM Matches
UNION ALL SELECT 'MatchPlayers', COUNT(*) FROM MatchPlayers;

-- 資料分布檢查：每個玩家的英雄池應該集中在 8 隻左右，時間應該散佈在 180 天內
SELECT PlayerId,
       COUNT(*)                    AS Games,
       COUNT(DISTINCT ChampionId)  AS Champions,
       MIN(GameDate)               AS Earliest,
       MAX(GameDate)               AS Latest,
       ROUND(AVG(CAST(Win AS float)) * 100, 2) AS WinRatePct
FROM MatchPlayers
GROUP BY PlayerId
ORDER BY PlayerId;
