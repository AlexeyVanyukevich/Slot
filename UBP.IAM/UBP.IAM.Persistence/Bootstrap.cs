using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using UBP.IAM.Persistence.Interfaces;
using UBP.IAM.Persistence.Repositories;
using UBP.IAM.Domain.Entities;
using UBP.IAM.Persistence.Contexts;
using UBP.IAM.Persistence.Options;
using Microsoft.Extensions.Hosting;

namespace UBP.IAM.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        Func<IServiceProvider, DbOptions> dbOptionsFactory)
    {
        services.AddRepositories();
        services.AddDbContext<AuthDbContext>((sp, builder) =>
        {
            var options = dbOptionsFactory(sp);

            builder.UseNpgsql(options.ConnectionString)
                   .UseSnakeCaseNamingConvention();
            builder.UseOpenIddict();
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISignInRepository, SignInRepository>();

        return services;
    }

    public static IdentityBuilder AddIdentity(
        this IServiceCollection services,
        Action<IdentityOptions> configure)
    {
        return services
            .AddIdentity<ApplicationUser, IdentityRole>(configure)
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
    }

    public static OpenIddictEntityFrameworkCoreBuilder UseAuthDbContext(
        this OpenIddictCoreBuilder builder)
    {
        return builder.UseEntityFrameworkCore()
                      .UseDbContext<AuthDbContext>();
    }

    public static IHost UsePersistence(this IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        using IServiceScope scope = host.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();
        return host;
    }
}
