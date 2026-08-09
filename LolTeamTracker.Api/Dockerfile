# ══════════ 第一階段：建置 ══════════
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 只先複製 .csproj，單獨跑 restore
COPY ["LolTeamTracker.Api/LolTeamTracker.Api.csproj", "LolTeamTracker.Api/"]
RUN dotnet restore "LolTeamTracker.Api/LolTeamTracker.Api.csproj"

# 套件還原完才複製原始碼
COPY . .
RUN dotnet publish "LolTeamTracker.Api/LolTeamTracker.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ══════════ 第二階段：執行 ══════════
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Linux 映像預設不含時區資料庫
RUN apt-get update \
 && apt-get install -y --no-install-recommends tzdata \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# 設定 ASP.NET Core 監聽所有網路介面，並使用 8080 埠口
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LolTeamTracker.Api.dll"]