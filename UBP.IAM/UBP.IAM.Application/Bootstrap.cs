using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Reflection;

using UBP.Core.Persistence.Database.Options;
using UBP.CQRS;
using UBP.IAM.Persistence;

namespace UBP.IAM.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Action<DbOptions>? configure = null)
    {
        services.AddPersistence(configure);
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