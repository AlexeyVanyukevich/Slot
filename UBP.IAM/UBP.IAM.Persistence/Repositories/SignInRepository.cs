using Microsoft.AspNetCore.Identity;

using UBP.IAM.Persistence.Interfaces;
using UBP.IAM.Domain.Entities;

namespace UBP.IAM.Persistence.Repositories;

internal sealed class SignInRepository(SignInManager<ApplicationUser> signInManager) : ISignInRepository
{
    public Task<bool> CanSignInAsync(ApplicationUser user)
    {
        return signInManager.CanSignInAsync(user);
    }

    public Task SignOutAsync()
    {
        return signInManager.SignOutAsync();
    }
}
