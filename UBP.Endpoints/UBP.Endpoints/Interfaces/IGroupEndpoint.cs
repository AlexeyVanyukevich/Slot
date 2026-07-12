namespace UBP.Endpoints.Interfaces;

public interface IGroupEndpoint<TGroup> : IEndpoint
    where TGroup : IEndpointGroup
{
}
