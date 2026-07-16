
using OpenIddict.Abstractions;

using UBP.IAM.API.Constants;
using UBP.IAM.API.Extensions;
using UBP.IAM.Application;
using UBP.Logging;
using UBP.OpenApi;
using UBP.OpenApi.Scalar;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.ConfigureAppOptions();
builder.Services.AddApplication();
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

builder.Services.AddOpenApiDocumentation("v1", options =>
{
    options.Title = "IAM API";
    options.Version = "v1";
    options.Description = "Identity and Access Management API";
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
app.MapOpenApiDocumentation();
app.MapRazorPages();
app.MapControllers();

await app.RunAsync();
