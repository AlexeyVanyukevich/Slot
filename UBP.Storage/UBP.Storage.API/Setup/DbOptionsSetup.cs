using Microsoft.Extensions.Options;

using UBP.Core.Persistence.Database.Options;

namespace UBP.Storage.API.Setup;

internal sealed class DbOptionsSetup(IConfiguration configuration) : IConfigureOptions<DbOptions>
{
    private const string ConnectionStringKey = "StorageDb";

    public void Configure(DbOptions options)
    {
        options.ConnectionString = configuration.GetConnectionString(ConnectionStringKey)!;
    }
}
