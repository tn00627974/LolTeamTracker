using FluentValidation;
using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Data;
using LolTeamTracker.Api.Filters;
using LolTeamTracker.Api.Middleware;
using LolTeamTracker.Api.Repositories;
using LolTeamTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Westwind.AspNetCore.LiveReload;


var builder = WebApplication.CreateBuilder(args);

// Add DbContext 
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});


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

// 全域例外處理 GlobalException
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// FluentValidation 驗證器註冊
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add services to the container.
builder.Services.AddLiveReload();
builder.Services.AddControllers(options =>
{
    // 全域驗證器 ( 在 Controller 之前執行過濾 ) 
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddScoped<IMatchAnalyzer,MatchAnalyzer>(); 
builder.Services.AddScoped<IRiotApiClient, RiotApiClient>();
builder.Services.AddScoped<ITeamRepository, EfTeamRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IStaticDataService, StaticDataService>();
builder.Services.AddScoped<IDataDragonClient, DataDragonClient>();
builder.Services.AddScoped<IStaticDataRepository, StaticDataRepository>();
builder.Services.AddScoped<IEfMatchPlayerRepository, EfMatchPlayerRepository>();
builder.Services.AddScoped<IEfQueueDefinitionRepository, EfQueueDefinitionRepository>();


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
    options.DescribeAllParametersInCamelCase(); // 路由都轉小寫
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseLiveReload(); 
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc"; // 最終網址 http://localhost:xxxx/redoc
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// top-level statement 產生的 Program 類別預設是 internal，
// WebApplicationFactory<Program> 要從 Tests 專案連進來，需要一個 public 的進入點。
public partial class Program { }
