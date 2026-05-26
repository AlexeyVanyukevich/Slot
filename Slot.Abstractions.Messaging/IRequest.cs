namespace Slot.Abstractions.Messaging;

public interface IRequest { }
public interface IRequest<TResponse> : IRequest { }

