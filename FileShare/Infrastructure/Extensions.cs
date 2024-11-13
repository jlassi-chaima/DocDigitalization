using Application;
using Application.Repository;
using DD.Infrastructure;
using Domain.Ports;
using Infrastructure.Adapters;
using Infrastructure.Behaviors;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
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
            // job for fetch new mails
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                //The key is typically used to identify and reference the job
                var jobkey = JobKey.Create(nameof(WriteFolder));
                //config background job
                options.AddJob<WriteFolder>(jobkey)
                        .AddTrigger(trigger => trigger.ForJob(jobkey).
                       WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(120).RepeatForever()));
            });
            //hosted service to the service collection
            services.AddQuartzHostedService();

            return services;
        }
       
        public static void AddFileShareInfrastructure(this WebApplicationBuilder builder)
        {
            //mediatR
            var applicationAssembly = typeof(ShareFile).Assembly;
            builder.AddInfrastructure(applicationAssembly);
           
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddBehaviors();
            builder.Services.AddScoped<MapsterMapper.Mapper>();
            builder.Services.AddScoped<IShareFolderRepository,ShareFolderRepository>();
            builder.Services.AddTransient<IUserGroupPort, UserGroupAdapter>();
            var config = builder.Configuration;

            _ = builder.Services.AddHttpClient("UserGroupClient", client =>
            {
                client.BaseAddress = new Uri(config["UserGroupApi:BaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            });



        }
        public static void UseFileShareInfrastructure(this WebApplication app)
        {

            app.UseInfrastructure(app.Environment);
        }
    }
}
