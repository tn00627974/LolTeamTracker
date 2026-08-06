/*
    開發環境測試資料：戰隊成員 6 筆
    來源：LolTeamTracker.Api/Data/Team/team.json

    用途：本機重建資料庫後快速補回測試資料。
          僅供開發使用，正式環境的資料由應用程式新增流程寫入。

    執行方式（任選一種）：
      1. SSMS 連線至 localhost,1434 後開啟本檔案執行
      2. docker exec lolteamtracker-mssql /opt/mssql-tools18/bin/sqlcmd \
             -S localhost -U sa -P "你的密碼" -C -d LolTeamTracker -i seed-players.sql

    前置條件：已執行 dotnet ef database update 建立 Players 資料表。

    本腳本可重複執行——以 Puuid 判斷是否已存在，不會產生重複資料。
*/

USE LolTeamTracker;
GO

MERGE INTO Players AS target
USING (VALUES
    ('8t5ALvzYjyDOFxAGdYO9VKhS5D5JiDi35XrydaFMRjkCuYssmb6qhYqRCzBQCxR3XX7vI-U0aBqsKw', N'TheSky',     'tw2'),
    ('bemk1rOXSFHkuJO2M8c6WjzGo0YL-g-BdtAMk6FdbjKho3-j69Y8rVYYPM1BJuUrpYZn-puUMwBPkQ', N'深邃紅月',   'tw2'),
    ('H2DSiQOA5lssNrBQTXq76WnOLReUmgcQFxS7Y3i81AUE21URsL-d16yP719iQQ4KB7NroGqeYSDSYg', N'地瓜飛拳',   '0530'),
    ('_ZUlG-3yvkXTyzwTJEDHeCIHppULJ3Wmgj7iGSOZAKGip4jQ5nz6cHxdbahg0VvfadFaCGjuKRFsMQ', N'艾藤o靜',    'tw2'),
    ('4d6PYmIA95SI-RR4pIY2mveXE3k6b4vEul2I__Kl_AN8AIQ7VdYncfVmy2iTTpiyfd3fjmWONb_Nog', N'控肉精靈',   'tw2'),
    ('MNIr03LV6IqLEjEWorQiur0eA2t7iiaOXMBJmou4auprFiwlCRHF69Zax96qaBxTPbCCYGi0hEgydA', N'微糖不會甜', '4206')
) AS source (Puuid, GameName, TagLine)
ON target.Puuid = source.Puuid

-- 已存在：更新名稱（模擬玩家改名的情境）
WHEN MATCHED AND (target.GameName <> source.GameName OR target.TagLine <> source.TagLine) THEN
    UPDATE SET
        target.GameName  = source.GameName,
        target.TagLine   = source.TagLine,
        target.UpdatedAt = GETUTCDATE()

-- 不存在：新增
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Puuid, GameName, TagLine, CreatedAt, UpdatedAt)
    VALUES (source.Puuid, source.GameName, source.TagLine, GETUTCDATE(), GETUTCDATE());
GO

SELECT Id, GameName, TagLine, LEN(Puuid) AS PuuidLength FROM Players ORDER BY Id;
GO
