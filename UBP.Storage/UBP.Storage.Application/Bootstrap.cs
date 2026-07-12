using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using UBP.CQRS;
using UBP.Storage.Persistence;

namespace UBP.Storage.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
    {
        services.AddPersistence(optionsAction);
        return services.AddMessaging(Assembly.GetExecutingAssembly());
    }
}
