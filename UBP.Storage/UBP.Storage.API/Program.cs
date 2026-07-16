using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using UBP.Auth;
using UBP.Endpoints;
using UBP.Logging;
using UBP.OpenApi;
using UBP.OpenApi.Scalar;
using UBP.Storage.Abstractions;
using UBP.Storage.API.Extensions;
using UBP.Storage.Application;
using UBP.Storage.Local;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.ConfigureAppOptions();
builder.Services.AddApplication();

builder.Services.AddLocalAssetStorage(
    sp => sp.GetRequiredService<IOptions<AssetStorageOptions>>().Value,
    sp => sp.GetRequiredService<IOptions<LocalAssetStorageOptions>>().Value);

builder.Services.AddIamAuthentication(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"]!;
    options.Audience = "storage_api";
});
builder.Services.AddAuthorization();

builder.Services.AddOpenApiDocumentation("v1", options =>
{
    options.Title = "Storage API";
    options.Version = "v1";
    options.Description = "Asset storage and management API";
});

builder.Services.AddEndpoints();

WebApplication app = builder.Build();

var localStorageOptions = app.Services.GetRequiredService<IOptions<LocalAssetStorageOptions>>().Value;
if (!string.IsNullOrEmpty(localStorageOptions.RootPath))
{
    Directory.CreateDirectory(localStorageOptions.RootPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.GetFullPath(localStorageOptions.RootPath)),
        RequestPath = "/assets"
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApiDocumentation();
app.MapEndpoints("/api");

await app.RunAsync();
