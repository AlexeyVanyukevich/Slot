using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using UBP.Core.Persistence.Database.Options;
using UBP.CQRS;
using UBP.Storage.Persistence;

namespace UBP.Storage.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Action<DbOptions>? configure = null)
    {
        services.AddPersistence(configure);
        return services.AddMessaging(Assembly.GetExecutingAssembly());
    }
}