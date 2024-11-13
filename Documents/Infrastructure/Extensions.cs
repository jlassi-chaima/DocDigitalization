using Application;
using Application.Dtos.Documents;
using Application.Features.AssignDocumentMangement;
using Application.Features.FeaturesDocument.DocToSharePoint;
using Application.Features.FeaturesDocument.Documents;
using Application.Helper;
using Application.Mappings;
using Application.Respository;
using Application.Services;
using Azure.Identity;
using Core.Events;
using DD.Infrastructure;
using DD.Infrastructure.Messaging;
using Domain.Documents;
using Domain.Ports;
using FluentValidation;
using Infrastructure.Adapters;
using Infrastructure.Behaviors;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Service;
using Infrastructure.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Nest;
using System.Net.Http.Headers;
using TT.Internet.Framework.Infrastructure.Filters;
using static Application.Features.ArchiveSerialNumbersFeature.AddArchiveSerialNumber.Handler;
using static Application.Features.ArchiveSerialNumbersFeature.DeleteArchiveSerialNumber.Handler;



namespace Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
           

            var url = configuration["ElasticsearchSettings:Url"];
            var defaultIndex = configuration["ElasticsearchSettings:Index"];

            var settings = new ConnectionSettings(new Uri(url))
                .PrettyJson()
                .DefaultIndex(defaultIndex);
            var client = new ElasticClient(settings);

            // Register ElasticClient as a singleton
            services.AddSingleton<IElasticClient>(client);
            var indexExistsResponse = client.Indices.Exists(defaultIndex);

            // Add Swagger generation
            

            if (!indexExistsResponse.Exists)
            {
                // Create the index with the appropriate mappings
                var createIndexResponse = client.Indices.Create(defaultIndex, c => c
                    .Map<Document>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Title)
                                .Name(n=>n.Content)
                                .Fields(f => f
                                    .Completion(c => c
                                        .Name("suggest")
                                        .Analyzer("standard")))))));

                //if (!createIndexResponse.IsValid)
                //{
                //    // Handle index creation failure
                //    throw new Exception($"Failed to create index: {createIndexResponse.ServerError.Error.Reason}");
                //}
            }
            //var createIndexResponse = client.Indices.Create(defaultIndex,
            //    index => index.Map<Document>(x => x.AutoMap())
            //);
            services.AddDbContext<DBContext>(opts =>
            {
                opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            }, ServiceLifetime.Scoped);
            services.Configure<OriginalsSettings>(configuration.GetSection("ArchiveSettings"));
            services.AddSignalR();
            services.AddHttpClient();
            //Quartz
            //services.AddQuartz(options =>
            //{
            //    options.UseMicrosoftDependencyInjectionJobFactory();
            //    //The key is typically used to identify and reference the job
            //    var jobkey = JobKey.Create(nameof(BackgroundJob));
            //    //config background job
            //    options.AddJob<BackgroundJob>(jobkey)
            //            .AddTrigger(trigger =>trigger.ForJob(jobkey).WithSchedule()
            //            //WithSimpleSchedule(schedule=>schedule.WithIntervalInSeconds(0).)));
            //});
            ////hosted service to the service collection
            //services.AddQuartzHostedService();

            return services;
        }
        public static void AddDocumentsInfrastructure(this WebApplicationBuilder builder)
        {
            //mediatR
            var applicationAssembly = typeof(DocumentApplication).Assembly;
            builder.AddInfrastructure(applicationAssembly);
            builder.Services.AddMassTransit(config =>
            {
                config.AddConsumers(applicationAssembly);
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMqOptions:Host"]);
                    cfg.ConfigureEndpoints(ctx, new DefaultEndpointNameFormatter("tag"));
                });
            });
            //Validator
            builder.Services.AddScoped(typeof(ValidationFilter<>));
            builder.Services.AddValidatorsFromAssemblyContaining<ArchiveValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<GuidValidator>();
            //repository


            builder.Services.AddTransient<IDocumentRepository,DocumentsRepository>();
            builder.Services.AddTransient<ITagRepository,TagRepository>();
            builder.Services.AddTransient<IDocumentTypeRepository, DocumentTypeRepository>();
            builder.Services.AddTransient<ICustomFieldRepository, CustomfieldRepository>();
            builder.Services.AddTransient<ICorrespondentRepository, CorrespondentRepository>();
            builder.Services.AddTransient<IDocumentNoteRepository, DocumentNoteRepository>();
            builder.Services.AddTransient<IStoragePathRepository, StoragePathRepository>();
            builder.Services.AddTransient<ITemplateRepository , TemplateRepository>();
            builder.Services.AddTransient<IViewRepository,ViewRepository>();
            builder.Services.AddTransient<ILogRepository, LogRepository>();
            builder.Services.AddTransient<IFileTasksRepository, FileTasksRepository>();
            builder.Services.AddSingleton<IElasticsearchRepository, ElasticsearchRepository>();
            builder.Services.AddTransient<IArchiveSerialNumberRepository, ArchiveSerialNumberRepository>();

            builder.Services.AddTransient<DownloadOriginalFile>();
            builder.Services.AddScoped<IEventPublisher, EventPublisher>();
            //ELASTIC SEARH 

            //  builder.Services.AddTransient<IValidator<Tag>, AddValidator>();
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddBehaviors();
            builder.Services.AddScoped<MapsterMapper.Mapper>();
            // service extract asn , upload files , download files, archive files
            builder.Services.AddTransient<ExtractASNservice>();
            builder.Services.AddTransient<ServiceUploadOCR>();
            builder.Services.AddTransient<ServiceUploadImage>();
            builder.Services.AddTransient<ServiceUploadWordFile>();
            builder.Services.AddTransient<ServiceUploadPowerPointFile>();
            builder.Services.AddTransient<ServiceUploadExcelFile>();
            builder.Services.AddTransient<ServiceUploadTextFile>();
            builder.Services.AddTransient<DownloadPDF>();
            builder.Services.AddTransient<EncryptionHelper>();

            builder.Services.AddTransient<ArchiveStoragePath>();
            // service assign 
            builder.Services.AddTransient<AssignTagToDocument>();
            builder.Services.AddTransient<AssignCorrespondentToDocument>();
            builder.Services.AddTransient<AssignDocumentTypeToDocument>();
            builder.Services.AddTransient<AssignStoragePathToDocument>();

            builder.Services.AddTransient<IAzurePort, AzureAdapter>();
            builder.Services.AddTransient<IGraphApiPort, GraphApiAdapter>();
            builder.Services.AddTransient<IUserGroupPort, UserGroupAdapter>();

            builder.Services.AddTransient<IUploadDocumentUseCase, UploadDocumentUseCase>();
            builder.Services.AddTransient<IGraphApiUseCase, GraphApiUseCase>();

            builder.Services.AddTransient<ILogService, LogService>();
            builder.Services.AddTransient<IUserGroupService, UserGroupService>();


            builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));
            builder.Services.Configure<SharePointOptions>(builder.Configuration.GetSection("SharePoint"));

            builder.Services.AddSingleton<GraphServiceClient>(sp =>
            {
                var azureAdOptions = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;

                var clientSecretCredential = new ClientSecretCredential(
                    azureAdOptions.TenantId,
                    azureAdOptions.ClientId,
                    azureAdOptions.ClientSecret
                );

                return new GraphServiceClient(clientSecretCredential);
            });
            var config = builder.Configuration;
            _ = builder.Services.AddHttpClient("UserClient", client =>
            {
                client.BaseAddress = new Uri(config["UserGroupApi:UserBaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            });
       
            _ = builder.Services.AddHttpClient("GroupClient", client =>
            {
                client.BaseAddress = new Uri(config["UserGroupApi:GroupBaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            });
            MapsterConfiguration.Configure();
        


        }
        public static void UseDocumentInfrastructure(this Microsoft.AspNetCore.Builder.WebApplication app)
    {
           
            app.UseInfrastructure(app.Environment);
    }

    }
}
