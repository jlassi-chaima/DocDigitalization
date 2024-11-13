using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationService(builder.Configuration);

builder.AddIdentityInfrastructure();
var app = builder.Build();
app.UseIdentityInfrastructure();

app.MapGet("/", () => "Hello World!");

app.Run();
