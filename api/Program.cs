using api.IServices;
using api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<IAccountInfoTest,AccountInfoTest>();
var app = builder.Build();
app.MapControllers();
app.Run();
