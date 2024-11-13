using Core.Events;
using DD.Infrastructure.Mapping.Mapster;
using DD.Infrastructure.Messaging;
using FluentValidation;
using Infrastructure.Logging.Serilog;
using Infrastructure.Middlewares;
using Infrastructure.Options;
using Infrastructure.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json.Serialization;


namespace DD.Infrastructure
{
    public static class Extensions
    {
       
        public static void AddInfrastructure(this WebApplicationBuilder builder, Assembly? applicationAssembly = null, bool enableSwagger = true)
        {
            var config = builder.Configuration;
            var appOptions = builder.Services.BindValidateReturn<AppOptions>(config);
            builder.ConfigureSerilog(appOptions.Name);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowAll",
                                  builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            if (applicationAssembly != null)
            {
                //
                builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddMapsterExtension(applicationAssembly);
                builder.Services.AddValidatorsFromAssembly(applicationAssembly);
                builder.Services.AddRouting(options => options.LowercaseUrls = true);
                builder.Services.AddMediatR(o => o.RegisterServicesFromAssembly(applicationAssembly));
             
            }
            if (enableSwagger) builder.Services.AddSwaggerExtension(config);
        }
        public static void UseInfrastructure(this WebApplication app, IWebHostEnvironment env, bool enableSwagger = true)
        {
           
            app.UseCors("AllowAll");
            app.MapControllers();
            if (enableSwagger) app.UseSwaggerExtension(env);
        }
    }
}
