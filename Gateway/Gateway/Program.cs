using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Validation.AspNetCore;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", builder =>
{
    //builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    builder.AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    builder.RequireAuthenticatedUser();
}));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builder => builder.AddRequestTransform(async context =>
    {

        var token = await context.HttpContext.GetTokenAsync(
            scheme: CookieAuthenticationDefaults.AuthenticationScheme,
            tokenName: Tokens.BackchannelAccessToken);


        context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(Schemes.Bearer, token);
    })
    );

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {

        options.SetIssuer("https://localhost:44313/");
        options.AddAudiences("rs_dataEventRecordsApi");

        options.UseIntrospection()
               .SetClientId("rs_dataEventRecordsApi")
               .SetClientSecret("dataEventRecordsSecret");

        options.UseSystemNetHttp();

        options.UseAspNetCore();
    });


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseHttpsRedirection();



app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

