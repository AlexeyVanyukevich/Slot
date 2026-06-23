using Microsoft.AspNetCore.Identity;

using UBP.IAM.Domain.Entities;

namespace UBP.IAM.Persistence.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByIdAsync(string id);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
}
