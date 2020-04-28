using Microsoft.Extensions.DependencyInjection;

namespace JStore
{
    public static class MvcExtensions
    {
        public static IServiceCollection AddJStore(this IServiceCollection services)
        {
            services.AddScoped(typeof(Repository<>));
            return services;
        }
    }
}
