using Microsoft.AspNetCore.Identity;

using UBP.IAM.Persistence.Interfaces;
using UBP.IAM.Domain.Entities;

namespace UBP.IAM.Persistence.Repositories;

internal sealed class UserRepository(UserManager<ApplicationUser> userManager) : IUserRepository
{
    public Task<ApplicationUser?> FindByIdAsync(string id)
    {
        return userManager.FindByIdAsync(id);
    }

    public Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
    {
        return userManager.IsEmailConfirmedAsync(user);
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        return userManager.CreateAsync(user, password);
    }
}
