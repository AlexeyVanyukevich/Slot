namespace UBP.CQRS;

public interface IRequest { }
public interface IRequest<TResponse> : IRequest { }

