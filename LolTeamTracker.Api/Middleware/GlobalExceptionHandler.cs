using LolTeamTracker.Api.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 先把例外記錄下來
            logger.LogError(exception, $"Unhandled exception occurred 追蹤代碼：{httpContext.TraceIdentifier}");

            var (status, title, detail) = exception switch
            {
                #region ResponseException For Client 
                // 429 限流、或 StatusCode 是 null 代表根本連不上網路）怎麼處理？
                HttpRequestException { StatusCode: System.Net.HttpStatusCode.TooManyRequests } => (
                    StatusCodes.Status429TooManyRequests,
                    "Too Many Requests",
                    "查詢過快，請稍後再試試看!"
                ),

                // 400
                HttpRequestException { StatusCode: System.Net.HttpStatusCode.BadRequest } => (
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    "請求參數格式不正確，請確認輸入內容"
                ),

                // 404 : 當使用者查詢時發生錯誤
                HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound } => (
                    StatusCodes.Status404NotFound,
                    "Player not found",
                    "找不到玩家，請確認輸入的名稱與 TagLine 是否正確"
                ),

                // Riot 回 401 或 403：是系統API Key逾期，呼叫端不需要知道細節一樣回傳 500 
                HttpRequestException { StatusCode: System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden } => (
                    StatusCodes.Status500InternalServerError,
                    "Server error",
                    "系統發生未預期的錯誤"
                ),
                
                // 其餘錯誤 
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Server error",
                    "系統發生未預期的錯誤" 
                )
                #endregion
            };

        // 依例外類型決定狀態碼
        var problemDetails = new ProblemDetails
            {
                Type = "about:blank",  
                Title = title,
                Status = status,
                Detail = detail,  
                Instance = httpContext.Request.Path
            };

            // 設定 HTTP 狀態碼
            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        
        }
    }
}

