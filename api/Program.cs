using api.Client;
using api.Crawler;
using api.Data;
using api.IServices;
using api.Services;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using api.IDbServices;
using api.DbServices;
using api.Stats;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (connectionString == null)
{
    throw new NullReferenceException("ConnectionString==null");
}
builder.Services.AddDbContext<AppDbContext>(o=>o.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddScoped<IAccountInfoTest,AccountInfoTest>();
builder.Services.AddScoped<ICrawlerService,CrawlerService>();
builder.Services.AddScoped<ICrawlPlayerDbService,CrawlPlayerDbService>();
builder.Services.AddScoped<IMatchDbService,MatchDbService>();
builder.Services.AddScoped<IPlayerService,PlayerService>();

builder.Services.AddScoped<IStatsService,StatsService>();

builder.Services.AddSingleton<RiotRateLimiter>();


builder.Services.AddHttpClient<IRiotClient,RiotClient>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

if (Environment.GetEnvironmentVariable("CRAWLER_ENABLED") == "true")
{
    builder.Services.AddHostedService<Crawler>();
    builder.Services.AddHostedService<StatsBcg>();

}

var app = builder.Build();
app.UseCors();
app.MapControllers();
app.Run();
