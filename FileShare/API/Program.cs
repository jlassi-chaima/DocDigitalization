using Infrastructure;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationService(builder.Configuration);
builder.AddFileShareInfrastructure();
var app = builder.Build();
app.UseFileShareInfrastructure();
app.MapGet("/", () => "Hello World!");

app.Run();
