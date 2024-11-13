using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TT.Internet.Framework.Infrastructure.Filters
{
    public static class Extensions
    {
        public static IServiceCollection AddFilters(this IServiceCollection services)
        {
            services.AddTransient(typeof(IValidator<>), typeof(ValidationFilter<>));
            return services;
        }
    }
}
