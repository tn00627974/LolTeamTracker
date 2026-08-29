/*
    參照資料：遊戲模式對照表（QueueDefinitions）

    來源：https://static.developer.riotgames.com/docs/lol/queues.json

    ⚠️ 這是「參照資料」不是測試資料——正式環境也需要它。
       沒有這張表，API 回傳的 gameMode 會變成「未知模式 (QueueId=870)」。

    與 seed-test-data.sql 的差別：
      本腳本不會刪除任何既有資料，可安全在任何環境重複執行。
      seed-test-data.sql 會 TRUNCATE 比賽資料，僅限開發環境。

    執行方式（任選一種）：
      1. SSMS 連線後開啟本檔案執行
      2. docker exec lolteamtracker-mssql /opt/mssql-tools18/bin/sqlcmd \
             -S localhost -U sa -P "你的密碼" -C -d LolTeamTracker -i seed-queue-definitions.sql

    前置條件：已執行 dotnet ef database update 建立 QueueDefinitions 資料表。

    冪等：以 Id 判斷是否已存在，跑幾次都不會撞主鍵，也不會覆蓋既有的名稱。
          Riot 新增模式時，在下面的 VALUES 補一列再跑一次即可——不需要改程式碼、不需要重新部署。
*/

USE LolTeamTracker;
GO

INSERT INTO QueueDefinitions (Id, Name, Description, UpdatedAt)
SELECT v.Id, v.Name, v.Description, SYSUTCDATETIME()
FROM (VALUES
    (400, N'Normal Draft Pick（一般選角）', N'5v5 一般模式，選角階段可禁用英雄'),
    (420, N'單雙積分 Solo/Duo Ranked',      N'5v5 排位，僅限單人或雙人組隊'),
    (430, N'Normal Blind Pick（一般盲選）',  N'5v5 一般模式，看不到對手選角'),
    (440, N'彈性積分 Flex Ranked',          N'5v5 排位，可多人組隊'),
    (450, N'大亂鬥 ARAM',                   N'嚎哭深淵隨機英雄，無路線概念'),
    (480, N'一般（超速衝點）',                N'Swiftplay 快節奏一般模式'),
    (750, N'Clash 盃',                      N'錦標賽制'),
    (870, N'人機對戰（入門）',                N'Co-op vs AI Intro Bot'),
    (900, N'無限亂鬥 ARURF',                 N'隨機英雄極速模式')
) AS v(Id, Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM QueueDefinitions q WHERE q.Id = v.Id);

-- PRINT 的參數只接受純量運算式，不能直接放子查詢（Msg 1046），
-- 所以先把 COUNT(*) 存進變數。整個 batch 是一起編譯的——
-- 這裡語法錯誤會讓上面的 INSERT 一併不執行。
DECLARE @Cnt int = (SELECT COUNT(*) FROM QueueDefinitions);
PRINT CONCAT('QueueDefinitions 目前共 ', @Cnt, ' 筆');
GO

SELECT Id, Name, Description FROM QueueDefinitions ORDER BY Id;
