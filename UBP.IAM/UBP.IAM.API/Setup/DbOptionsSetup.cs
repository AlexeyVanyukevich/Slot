using Microsoft.Extensions.Options;

using UBP.Core.Persistence.Database.Options;

namespace UBP.IAM.API.Setup;

internal sealed class DbOptionsSetup(IConfiguration configuration) : IConfigureOptions<DbOptions>
{
    const string ConnectionStringKey = "AuthDb";

    public void Configure(DbOptions options)
    {
        options.ConnectionString = configuration.GetConnectionString(ConnectionStringKey)!;
    }
}
