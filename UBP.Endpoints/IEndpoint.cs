using Microsoft.AspNetCore.Routing;

namespace UBP.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder builder);
}
