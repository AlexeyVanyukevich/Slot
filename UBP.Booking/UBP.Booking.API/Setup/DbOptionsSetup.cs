using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using UBP.Core.Persistence.Database.Options;

namespace UBP.Booking.API.Setup;

internal sealed class DbOptionsSetup(IConfiguration configuration) : IConfigureOptions<DbOptions>
{
    private const string ConnectionStringKey = "BookingDb";

    public void Configure(DbOptions options)
    {
        options.ConnectionString = configuration.GetConnectionString(ConnectionStringKey)!;
    }
}
