using UBP.Endpoints.Interfaces;

namespace UBP.Endpoints.Versioning;

public abstract class VersionGroup(int version) : IEndpointGroup
{
    public virtual string Prefix => $"/v{version}";
}
