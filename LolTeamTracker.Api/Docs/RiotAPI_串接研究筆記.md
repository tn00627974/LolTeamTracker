# ✅ 1. 申請 Riot 開發者 API Key

1. 註冊 Riot 帳號並前往 Riot 開發者平台  https://developer.riotgames.com/
2. 申請開發者 Key（**開發用 Key 有效 24 小時**，正式環境需審核）
   - API KEY : [[Ulysses/GitHub專案/RiotServer/Riot API Key|你申請的API KEY]]
3. 取得你的 `X-Riot-Token`，後續 API 都要用這個做身份驗證

📌 Step 1. 用 Riot ID 查 puuid
Simple :

```
GET 
https://asia.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}
Header:
X-Riot-Token: <你的 API Key>
```

- {gameName} : `真實遊戲ID`
- {tagLine} : `#tw2`

✅GET

```
https://asia.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{你的id}/TW2
```

```json
{
    "puuid": "bemk1rOXSFHkuJO2M8c6WjzGo0YL-g-BdtAMk6FdbjKho3-j69Y8rVYYPM1BJuUrpYZn-puUMwBPkQ",
    "gameName": "深邃紅月",
    "tagLine": "tw2"
}
```

📌 Step 2. 用 puuid 查比賽列表
GET ✅要將asia.api.riotgames.com 更換成 **sea**.api.riotgames.com

```
https://sea.api.riotgames.com/lol/match/v5/matches/by-puuid/{{puuid}}}/ids?start=0&count=10
```

![[Ulysses/成長筆記本/資料工程師/附件圖片檔/Pasted image 20250705215341.png]]
![[Ulysses/成長筆記本/資料工程師/附件圖片檔/Pasted image 20250705223330.png]]
![[Ulysses/成長筆記本/資料工程師/附件圖片檔/Pasted image 20250706200007.png]]

| 地區/伺服器      | Routing Region（用於 Match API） |
| ---------------- | -------------------------------- |
| 台灣、香港、越南 | **sea**（Southeast Asia）  |
| 韓國             | **asia**                   |
| 日本             | **asia**                   |
| 歐洲             | **europe**                 |
| 北美             | **americas**               |

📌 Step 3. 用 puuid 查比賽列表細節

Simple :

```
https://sea.api.riotgames.com/lol/match/v5/matches/{matchId}
```

- {matchId} : `遊戲對戰編號`

✅GET

```
https://sea.api.riotgames.com/lol/match/v5/matches/TW2_316231903
```

---

# 📦 Riot 官方版本查詢（找出當前版本）

GET

```
https://ddragon.leagueoflegends.com/api/versions.json
```

# 📦 Riot 資料 JSON

https://ddragon.leagueoflegends.com/cdn/15.13.1/data/zh_TW/champion.json
https://ddragon.leagueoflegends.com/cdn/15.13.1/data/zh_TW/summoner.json
https://ddragon.leagueoflegends.com/cdn/15.13.1/data/zh_TW/item.json

從 Riot Match API 中取得的每場資料裡，這些都可以用 Riot 提供的 JSON 靜態資料對應：

- `championName`: 英雄英文名稱（直接拼在 `.../champion/{name}.png`）
- `summoner1Id` / `summoner2Id`: 對應 spell ID，你要用 spell.json 查名稱
- `item0` ~ `item6`: 直接就是 item 的整數 ID
- `perks`: 符文需用 perks.json 解析

| 你想顯示的內容 | 資料來源                          | 圖片位置                                 |
| -------------- | --------------------------------- | ---------------------------------------- |
| 英雄頭像       | Match API 中的`championName`    | `/img/champion/{name}.png`             |
| 技能圖示       | `summoner1Id` / `summoner2Id` | `/img/spell/{spellName}.png`（需轉換） |
| 道具圖示       | `item0 ~ item6`                 | `/img/item/{itemId}.png`               |
| 符文圖示       | `perks` 欄位解析後對應          | `/img/perk-images/...`                 |

## 🛠️ 使用 Postman 直接撈取最新版本號

### 步驟如下：

### 🔹 1. 建立第一個請求：取得版本清單

- 方法：`GET`
- URL：`https://ddragon.leagueoflegends.com/api/versions.json

### 🔹 2. 在此請求中加入 Pre-request Script（如下）

```javascript
// 這段程式碼放在 Pre-request Script 中會在請求前執行
pm.sendRequest("https://ddragon.leagueoflegends.com/api/versions.json", function (err, res) {
    if (!err) {
        const versionsArray = res.json();      // 把 response 轉成陣列
        const latestVersion = versionsArray[0]; // 取得最新版本
        pm.environment.set("versions", latestVersion); // 設定成環境變數
        console.log("最新版本為:", latestVersion);
    } else {
        console.error("錯誤取得版本:", err);
    }
});
```

### 🔹 3. 建立後續請求（使用該版本）

- 方法：`GET`
- URL：

```
`https://ddragon.leagueoflegends.com/cdn/{{versions}}/data/zh_TW/champion.json`
```

其他請求依樣畫葫蘆，把 `{{versions}}` 當作 URL 的一部分使用即可。

### 🔹4. 其餘 JSON API : 套入環境變數}

```javascript
https://ddragon.leagueoflegends.com/cdn/{{versions}}/data/zh_TW/champion.json
https://ddragon.leagueoflegends.com/cdn/{{versions}}/data/zh_TW/summoner.json
https://ddragon.leagueoflegends.com/cdn/{{versions}}/data/zh_TW/item.json
```

## 📌 備註：

- 你要先執行一次「版本取得」的 request，這樣 `versions` 變數才會被設定。
- 請確認使用的是 **Environment Variables**（環境變數），而不是 Global 或 Collection 變數。
- 你也可以改成用 `Collection Pre-request Script`，讓其他請求都共用。

---

## ✅ 開發總目標

打造一個簡易網頁平台，能自動查詢你朋友（6～10人）的戰績，並整合在一個畫面上查看。

---

## ✅ TODO LIST + 教學分解

### 🟧 第 1 階段：基礎資料準備與設計

| 步驟 | 項目                                                     | 教學說明                                                                                               |
| ---- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| ✅ 1 | 收集所有團員的`gameName + tagLine` → 轉換為 `puuid` | 使用 Riot API`/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}`                             |
| ✅ 2 | 建立 puuid 名單（JSON）                                  | 可放在本地 JSON 檔或資料庫，格式如下：`[{ "name": "深邃紅月", "tag": "tw2", "puuid": "xxxx" }, ...]` |
| ✅ 3 | 申請 Riot 開發者 API 金鑰                                | [https://developer.riotgames.com/](https://developer.riotgames.com/)每日更新一次 Key                    |
|      |                                                          |                                                                                                        |

---

### 🟨 第 2 階段：後端 API 建構（C# ASP.NET 建議）

| 步驟 | 項目                                                              | 教學說明                                                                      |
| ---- | ----------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| 🔧 4 | 建立 ASP.NET Web API 專案（.NET 8）                               | 建議使用`dotnet new webapi` 或 Visual Studio 建立                           |
| 🔧 5 | 建立服務`RiotApiService`，封裝 Riot API 呼叫邏輯                | 3 層邏輯：① 查 puuid → matchIds② 查 matchId → match info③ 過濾出自己戰績 |
| 🔧 6 | 建立 Controller`/api/match/{puuid}` → 回傳該玩家最近 10 場資料 | 使用`HttpClient` 進行請求，格式化回傳 JSON                                  |
| 🔧 7 | 建立一個`/api/team` API → 迴圈查詢所有團員資料                 | 可以並行查詢 6～10 個 puuid 的比賽結果                                        |

---

### 🟩 第 3 階段：快取與排程（選用）

| 步驟   | 項目                                              | 教學說明                                   |
| ------ | ------------------------------------------------- | ------------------------------------------ |
| ⏱️ 8 | 將查詢結果快取至記憶體 / Redis / JSON 檔案        | 避免過度打 Riot API，可 15~30 分鐘更新一次 |
| ⏱️ 9 | 加入自動更新排程（BackgroundService 或 Hangfire） | 定時更新所有 puuid 的比賽資料              |

---

### 🟦 第 4 階段：前端資料展示

| 步驟  | 項目                                                     | 教學說明                                           |
| ----- | -------------------------------------------------------- | -------------------------------------------------- |
| 💻 10 | 建立簡單網頁頁面（用 ASP.NET MVC、Razor Pages 或 React） | 顯示每位玩家卡片，內含最近 10 場對戰紀錄           |
| 💻 11 | 呼叫`/api/team` API 並用 JavaScript 顯示資料           | 用 fetch() 抓資料，渲染列表                        |
| 💻 12 | 美化介面（建議用 Tailwind CSS）                          | 可加入卡片、英雄頭像、勝敗配色、KDA 字體強調等樣式 |

---

### 🟪 第 5 階段：部署與優化（可選）

| 步驟  | 項目                                      | 教學說明                                     |
| ----- | ----------------------------------------- | -------------------------------------------- |
| 🚀 13 | 本地測試無誤後，部署到 VPS / 雲端         | 可用 Windows Server + IIS、或 Azure Web App  |
| 🔐 14 | 加入錯誤處理與 API 過載保護               | 若有速率限制，需加入 retry、封鎖機制等       |
| 📈 15 | 可加上資料分析（例如每人勝率、MVP場次等） | 額外統計功能：勝場率、平均 KDA、最常玩英雄等 |

---

## 🛠️ 最小可執行版本（MVP）

你只需要完成前面 7 步，就能完成「團隊戰績總表」的基礎功能 ✅

---

## 📦 我可以提供的資源（你只要指定）

1. ✅ C# ASP.NET Web API 專案模板（含 Riot 封裝服務）
2. ✅ JavaScript / Razor 前端頁面 + Tailwind 樣式
3. ✅ Riot API 呼叫工具類別（含 PUUID/Match 封裝）
4. ✅ Redis 快取與排程更新範例（選用）
5. ✅ 完整部署教學流程（若需公開上網）

---

要開發一個像 OP.GG 這樣的遊戲數據分析平台，你可以把學習與開發分成以下幾大模組來規劃，以下是詳細的 **開發學習路線圖**，適合你這樣已有 C# 與資料工程背景的工程師：

### 🔧 一、系統架構規劃階段

**目標：理解 OP.GG 的系統組成與流量結構**

| 模組         | 內容說明                                                                        |
| ------------ | ------------------------------------------------------------------------------- |
| 功能模擬     | 遊戲資料查詢、排行榜、使用者分析、API匯入、帳號系統、UI互動與圖表               |
| 架構設計     | 分層架構（前端、後端、資料庫、API、爬蟲/匯入系統）                              |
| 架構選擇     | 單頁式應用 SPA（React/Vue）或 ASP.NET MVC、.NET Web API、Redis 快取層、CDN 前置 |
| 資料來源分析 | 官方 API（如 Riot Games API）、非公開資料（爬蟲或第三方匯入）                   |

---

### 🌐 二、前端開發技能

| 項目      | 技術學習                             | 推薦資源                                                                        |
| --------- | ------------------------------------ | ------------------------------------------------------------------------------- |
| 網頁框架  | React（或 Next.js）/ Vue             | [React 官方教學](https://chatgpt.com/c/f)、[Vue Mastery](https://chatgpt.com/c/f) |
| 圖表呈現  | Chart.js / Recharts / ECharts        | [Recharts 文檔](https://chatgpt.com/c/f)                                         |
| UI 設計   | Tailwind CSS / Shadcn UI / Bootstrap | [Tailwind Cheat Sheet](https://chatgpt.com/c/f)                                  |
| SEO / SSR | Next.js / Nuxt.js                    | [Next.js 教學](https://chatgpt.com/c/f)                                          |

---

### 🧠 三、後端開發技能（C# or Node.js）

| 項目         | 技術學習                                 | 推薦資源                                 |
| ------------ | ---------------------------------------- | ---------------------------------------- |
| API 架設     | ASP.NET Core Web API / Node.js + Express | [官方教學](https://chatgpt.com/c/f)       |
| 快取設計     | Redis（用來避免過度查詢 API）            | [Redis 速成教學](https://chatgpt.com/c/f) |
| JWT 登入系統 | ASP.NET Identity / Firebase Auth         | [JWT 教學](https://chatgpt.com/c/f)       |
| 排程任務     | Hangfire / Quartz.NET / Node-cron        | [Hangfire 教學](https://chatgpt.com/c/f)  |

---

### 🗃️ 四、資料與爬蟲系統（資料來源處理）

|     項目     |                     技術學習                     |             備註             | 執行 |                                      限制                                      |
| :-----------: | :----------------------------------------------: | :--------------------------: | :--: | :-----------------------------------------------------------------------------: |
| 官方 API 串接 |           Riot Games API（需申請 Key）           |       有限速，注意快取       |  ✅  | #### Rate Limits20 requests every 1 seconds(s)  100 requests every 2 minutes(s) |
|  Python 爬蟲  | Requests + BeautifulSoup / Selenium / Playwright | 可抓戰績、ID、排行等非官方頁 |      |                                                                                |
|  定時抓資料  |     Celery / Cron / .NET Background Service     |        定時更新資料庫        |      |                                                                                |
|   資料清洗   |              Pandas / LINQ / Regex              |  處理錯誤資料或非結構化內容  |      |                                                                                |

---

### 🛢️ 五、資料庫設計與分析

| 項目             | 技術學習                           | 重點                                 |
| ---------------- | ---------------------------------- | ------------------------------------ |
| 資料庫選型       | PostgreSQL / SQL Server            | 支援 JSON/地理資料索引者佳           |
| 資料表設計       | 使用正規化 / 指標儲存 / Index 優化 | 分析用欄位要額外設計（KDA、WinRate） |
| 資料倉儲（可選） | DuckDB / ClickHouse / BigQuery     | 做高效查詢用（可延伸）               |

---

### 📈 六、數據分析與推薦系統（高階）

| 項目           | 技術學習                          | 說明                   |
| -------------- | --------------------------------- | ---------------------- |
| 資料統計       | Python Pandas / R                 | 分析勝率、角色搭配等   |
| ML 模型        | Scikit-learn / TensorFlow（進階） | 勝率預測、自動建議角色 |
| 使用者行為追蹤 | GA4 / PostHog / Mixpanel          | 分析熱門頁面與互動行為 |

---

### ☁️ 七、部署與運維

| 項目          | 技術學習                                                   | 推薦工具                                  |
| ------------- | ---------------------------------------------------------- | ----------------------------------------- |
| 雲端部署      | Vercel / Netlify（前端）、Render / Fly.io / Linode（後端） | 小專案可用免費層                          |
| Docker 容器化 | Docker + Docker Compose                                    | [Docker 官方指南](https://chatgpt.com/c/f) |
| CI/CD         | GitHub Actions / Azure DevOps                              | 自動部署前後端                            |
| 監控          | Uptime Kuma / Grafana / Prometheus                         | 可簡單整合異常通知                        |

---

### 🧪 八、推薦開發順序（學習實作建議）

1. ✅ 先從 Riot API 資料匯入與儲存開始（用 .NET + Redis + PostgreSQL）
2. ✅ 接著建立排行榜、查詢頁，開發 Web API + 前端頁面（React + Tailwind）
3. ✅ 引入快取（Redis）與排程更新（Hangfire）
4. ✅ 美化 UI、導入圖表呈現戰績分析
5. ✅ 若有餘力，做角色推薦（簡易機器學習或統計模型）
