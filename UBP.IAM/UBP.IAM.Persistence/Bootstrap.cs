using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using UBP.Core.Persistence.Database.Options;
using UBP.Core.Persistence.EF.PostgreSQL;
using UBP.IAM.Domain.Entities;
using UBP.IAM.Persistence.Contexts;
using UBP.IAM.Persistence.Interfaces;
using UBP.IAM.Persistence.Repositories;

namespace UBP.IAM.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        Action<DbOptions>? configure = null)
    {
        services.AddRepositories();
        services.AddPostgresEFDbContext<AuthDbContext, DbOptions>(configure);

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
        Action<IdentityOptions>? configure = null)
    {
        IdentityBuilder builder = configure is null
            ? services.AddIdentity<ApplicationUser, IdentityRole>()
            : services.AddIdentity<ApplicationUser, IdentityRole>(configure);

        return builder
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