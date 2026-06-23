using UBP.IAM.Domain.Entities;

namespace UBP.IAM.Persistence.Interfaces;

public interface ISignInRepository
{
    Task<bool> CanSignInAsync(ApplicationUser user);
    Task SignOutAsync();
}
