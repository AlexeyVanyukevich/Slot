using Microsoft.Extensions.Options;

using UBP.Storage.Local;

namespace UBP.Storage.API.Setup;

internal sealed class LocalAssetStorageOptionsSetup(IConfiguration configuration) : IConfigureOptions<LocalAssetStorageOptions>
{
    private const string SectionName = "LocalStorage";

    public void Configure(LocalAssetStorageOptions options)
    {
        configuration.GetSection(SectionName).Bind(options);
    }
}
