using Application.Features.FeaturesDocument;
using Infrastructure;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddDocument.Handler).Assembly));
builder.Services.AddApplicationService(builder.Configuration);

builder.AddDocumentsInfrastructure();
var app = builder.Build();
app.UseDocumentInfrastructure();
app.UseHttpsRedirection();
app.MapGet("/docs", () => "");
app.MapHub<Progress>("/progress");


app.Run();

