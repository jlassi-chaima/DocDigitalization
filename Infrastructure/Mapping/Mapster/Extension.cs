using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace DD.Infrastructure.Mapping.Mapster
{
    public static class Extension
    {
        public static IServiceCollection AddMapsterExtension(this IServiceCollection services, Assembly coreAssembly)
        {
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(coreAssembly);
            var mapperConfig = new Mapper(typeAdapterConfig);
            services.AddSingleton<IMapper>(mapperConfig);
            return services;
        }
    }
}
