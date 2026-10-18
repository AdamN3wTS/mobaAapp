using api.Client;
using api.IServices;
using api.Services;
DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<IAccountInfoTest,AccountInfoTest>();
builder.Services.AddHttpClient<IRiotClient,RiotClient>();

var app = builder.Build();
app.MapControllers();
app.Run();
