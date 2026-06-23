using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using System.Reflection;

using UBP.CQRS;
using UBP.IAM.Persistence;
using UBP.IAM.Persistence.Options;
using Microsoft.Extensions.Hosting;

namespace UBP.IAM.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Func<IServiceProvider, DbOptions> dbOptionsFactory)
    {
        services.AddPersistence(dbOptionsFactory);
        return services.AddMessaging(Assembly.GetExecutingAssembly());
    }

    public static IdentityBuilder AddIdentity(this IServiceCollection services)
    {
        return services.AddIdentity(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        });

    }

    public static OpenIddictBuilder AddOpenIdConnect(this IServiceCollection services)
    {
        return services.AddOpenIddict().AddCore(options => options.UseAuthDbContext());
    }

    public static void UseApplication(this IHost host, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            host.UsePersistence();
        }
    }
}
