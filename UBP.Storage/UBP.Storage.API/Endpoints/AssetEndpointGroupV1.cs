using UBP.Endpoints.Versioning;

namespace UBP.Storage.API.Endpoints;

internal sealed class AssetEndpointGroupV1 : V1Group
{
    public override string Prefix => $"{base.Prefix}/assets";
}
