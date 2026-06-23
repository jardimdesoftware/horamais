using System.Threading.Tasks;

namespace Back.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<(bool Success, string UserId, string[] Errors)> CreateUserAsync(string email, string password, string role, bool emailConfirmed = true);
    Task<(bool Success, string[] Errors)> UpdateUserAsync(string userId, string newEmail, string? newPassword);
}

