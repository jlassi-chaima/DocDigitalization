using Application;
using Infrastructure.Behaviors;
using Infrastructure.Persistence;
using MediatR;
using DD.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Repository;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Domain.User;
using Domain.Ports;
using Infrastructure.Adapters;
using System.Net.Http.Headers;


namespace Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DBContext>(opts =>
            {
                opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            });


            return services;
        }
        public static void AddIdentityInfrastructure(this WebApplicationBuilder builder)
        {
            // Add Identity services
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DBContext>()
                .AddDefaultTokenProviders();
            //mediatR
            var applicationAssembly = typeof(IdentityApplication).Assembly;
            builder.AddInfrastructure(applicationAssembly);
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddBehaviors();
            builder.Services.AddScoped<MapsterMapper.Mapper>();
            // repository 
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IUISettingsRepository, UISettingsRepository>();
            builder.Services.AddTransient<IGroupRepository, GroupRepository>();
            builder.Services.AddTransient<ISendEmailPort, SendEmailAdapter>();
            
            MapsterConfiguration.Configure();
        }
        public static void UseIdentityInfrastructure(this WebApplication app)
        {

            app.UseInfrastructure(app.Environment);
        }
    }
}
