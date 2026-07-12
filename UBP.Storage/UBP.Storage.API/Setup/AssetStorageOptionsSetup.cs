using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using UBP.Storage.Abstractions;

namespace UBP.Storage.API.Setup;

internal sealed class AssetStorageOptionsSetup(IConfiguration configuration) : IConfigureOptions<AssetStorageOptions>
{
    private const string SectionName = "AssetStorage";

    public void Configure(AssetStorageOptions options)
    {
        var section = configuration.GetSection(SectionName);

        foreach (var extension in section.GetSection(nameof(AssetStorageOptions.AllowedExtensions)).Get<string[]>() ?? [])
        {
            options.AllowedExtensions.Add(extension);
        }

        options.MaxSizeBytes = section.GetValue<long>(nameof(AssetStorageOptions.MaxSizeBytes));
    }
}
