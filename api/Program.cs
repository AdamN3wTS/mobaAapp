using api.Client;
using api.Crawler;
using api.Data;
using api.IServices;
using api.Services;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using api.IDbServices;
using api.DbServices;

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


builder.Services.AddHttpClient<IRiotClient,RiotClient>();

if (Environment.GetEnvironmentVariable("CRAWLER_ENABLED") == "true")
{
    builder.Services.AddHostedService<Crawler>();
}

var app = builder.Build();
app.MapControllers();
app.Run();
