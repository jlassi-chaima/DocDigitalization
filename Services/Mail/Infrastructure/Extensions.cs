using Application;
using Application.Repository;
using DD.Infrastructure;

using Infrastructure.Behaviors;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using MailKit.Net.Imap;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Gotenberg.Sharp.API.Client.Domain.Settings;

using Gotenberg.Sharp.API.Client.Extensions;
using Quartz;
using Infrastructure.Tasks.Mail;
using Application.Consumers.RestAPIDocuments.Endpoints;
using Domain.Ports;
using Infrastructure.Adapters;
using System.Net.Http.Headers;
using Application.Services;
using Infrastructure.Services;


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
            //convert mail to pdf using docker Gotenberg
            services.Configure<GotenbergSharpClientOptions>(configuration.GetSection("GotenbergSharpClient"));
            services.AddGotenbergSharpClient();
            //// job for fetch new mails
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                //The key is typically used to identify and reference the job
                var jobkey = JobKey.Create(nameof(Process_mail));
                //config background job
                options.AddJob<Process_mail>(jobkey)
                        .AddTrigger(trigger => trigger.ForJob(jobkey).
                       WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(120).RepeatForever()));
            });
            //hosted service to the service collection
            services.AddQuartzHostedService();

            return services;
        }
        public static void AddMailInfrastructure(this WebApplicationBuilder builder)
        {
            //mediatR
            var applicationAssembly = typeof(MailApplication).Assembly;
            builder.AddInfrastructure(applicationAssembly);
            builder.Services.AddMassTransit(config =>
            {
                config.AddConsumers(applicationAssembly);
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMqOptions:Host"]);
                    cfg.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter("catalog", false));
                });
            });
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddBehaviors();
            builder.Services.AddScoped<MapsterMapper.Mapper>();
            builder.Services.AddScoped<ImapClient>();
            builder.Services.AddTransient<IMailAccountRepository, MailAccountRepository>();
            builder.Services.AddTransient<IMailRuleRepository, MailRuleRepository>();
            builder.Services.AddTransient<TagsList>();
            builder.Services.AddTransient<IUserGroupPort, UserGroupAdapter>();
            builder.Services.AddTransient<ICorrespondentPort, CorrespondentAdapter>();
            builder.Services.AddTransient<ICorrespondentService, CorrespondentsService>();


            var config = builder.Configuration;

            _ = builder.Services.AddHttpClient("UserGroupClient", client =>
            {
                client.BaseAddress = new Uri(config["UserGroupApi:BaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            });
           
            _ = builder.Services.AddHttpClient("CorrespondentsClient", client =>
            {
                client.BaseAddress = new Uri(config["CorrespondentsApi:BaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            });


        }
        public static void UseMailInfrastructure(this WebApplication app)
        {

            app.UseInfrastructure(app.Environment);
        }
    }
}
