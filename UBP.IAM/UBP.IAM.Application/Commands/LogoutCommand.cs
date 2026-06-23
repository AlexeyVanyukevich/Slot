using UBP.CQRS;
using UBP.IAM.Persistence.Interfaces;

namespace UBP.IAM.Application.Commands;

public record LogoutCommand : IRequest;

internal sealed class LogoutCommandHandler(ISignInRepository signInRepository) : IRequestHandler<LogoutCommand>
{
    public async Task HandleAsync(LogoutCommand request, CancellationToken cancellationToken = default)
    {
        await signInRepository.SignOutAsync();
    }
}
