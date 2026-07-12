using Microsoft.AspNetCore.Routing;

namespace UBP.Endpoints.Interfaces;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder builder);
}
