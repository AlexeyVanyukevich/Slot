using UBP.Endpoints.Versioning;

namespace UBP.Booking.API.Endpoints;

internal sealed class SlotEndpointGroupV1 : V1Group
{
    public override string Prefix => $"{base.Prefix}/slots";
}
