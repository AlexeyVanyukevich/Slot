
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using UBP.IAM.API.Constants;
using UBP.IAM.API.Extensions;
using UBP.IAM.Application;
using UBP.IAM.Persistence.Options;
using UBP.Logging;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.ConfigureAppOptions();
builder.Services.AddApplication(sp => sp.GetRequiredService<IOptions<DbOptions>>().Value);
builder.Services.AddIdentity();

builder.Services.AddOpenIdConnect()
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetUserinfoEndpointUris("/connect/userinfo")
               .SetLogoutEndpointUris("/connect/logout");

        options.AllowAuthorizationCodeFlow()
               .RequireProofKeyForCodeExchange();

        options.AllowRefreshTokenFlow();

        options.RegisterScopes(
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OfflineAccess,
            Scopes.Api);

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddRazorPages();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseApplication(app.Environment);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

await app.RunAsync();
