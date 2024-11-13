using Infrastructure;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationService(builder.Configuration);
builder.AddMailInfrastructure();
var app = builder.Build();
app.UseMailInfrastructure();
app.MapGet("/", () => "Hello World!");

app.Run();
