using LolTeamTracker.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Westwind.AspNetCore.LiveReload;


var builder = WebApplication.CreateBuilder(args);

// "AccountBaseUrl": "https://asia.api.riotgames.com",
builder.Services.AddHttpClient("Account", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RiotApi:AccountBaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("X-Riot-Token",
        builder.Configuration["RiotApi:ApiKey"]);
});

// "RegionBaseUrl": "https://sea.api.riotgames.com"
builder.Services.AddHttpClient("Match", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RiotApi:RegionBaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("X-Riot-Token", 
        builder.Configuration["RiotApi:ApiKey"]);
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLiveReload();
builder.Services.AddHttpClient<MatchAnalyzer>(); // 註冊 MatchAnalyzer 
builder.Services.AddScoped<RiotApiService>(); // 註冊 RiotApiService 
builder.Services.AddScoped<RiotDataDownloader>(provider =>
    new RiotDataDownloader(
        provider.GetRequiredService<IHttpClientFactory>(),
        provider.GetRequiredService<IWebHostEnvironment>()
    ));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Swagger 中介軟體
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Swagger 文件的 API資訊與描述
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "20250712v1",
        Title = "RIOT API",
        Description = "An ASP.NET Core Web API for LolTeamTracker", 
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact 
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });

    // Swagger API網址加入XML註解
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});



var app = builder.Build();

// 測試：呼叫 RunAsync()
//using (var scope = app.Services.CreateScope())
//{
//    var analyzer = scope.ServiceProvider.GetRequiredService<MatchAnalyzer>();
//    await analyzer.RunAsync(); // 執行分析
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseLiveReload(); // 加這行
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc"; // 最終網址 http://localhost:xxxx/redoc
    });
    app.UseDeveloperExceptionPage(); // 顯示詳細錯誤
}

//app.UseStaticFiles();  // 暫時不用 wwwroot\StaticFiles 

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
