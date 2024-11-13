using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);
var reverseProxyConfig = builder.Configuration.GetSection("ReverseProxy") ?? throw new ArgumentException("ReverseProxy section is missing!");
var instance = builder.Configuration.GetValue<string>("AzureAd:Instance");
var tenantId = builder.Configuration.GetValue<string>("AzureAd:TenantId");
var clientId = builder.Configuration.GetValue<string>("AzureAd:ClientId");
var version = builder.Configuration.GetValue<string>("AzureAd:Version");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{instance}{tenantId}/{version}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = $"{instance}{tenantId}/{version}",
            ValidAudience = clientId
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully.");
                Console.WriteLine($"Issuer: {context.SecurityToken.Issuer}");

                Console.WriteLine($"Audience: {context.SecurityToken}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    // This is a default authorization policy which requires authentication
    options.AddPolicy("CookieAuthenticationPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});
builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseRouting();
app.UseHttpsRedirection();




app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        Console.WriteLine("User is not authenticated");
    }
    await next();
});
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapReverseProxy(proxyPipeline =>
//    {
//        proxyPipeline.UseAuthorization();
//        proxyPipeline.Use(async (context, next) =>
//        {
//            var endpoint = context.GetEndpoint();
//            if (endpoint?.Metadata?.GetMetadata<AuthorizationPolicy>() != null)
//            {
//                var authResult = await context.AuthenticateAsync();
//                if (!authResult.Succeeded)
//                {
//                    Console.WriteLine("Authentication failed: {Error}", authResult.Failure);
//                    await context.ChallengeAsync();
//                    context.Response.StatusCode = 401;
//                    await context.Response.CompleteAsync();
//                    return;
//                }
//            }
//            await next();
//        });
//    });
//});
app.MapReverseProxy();
app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();


