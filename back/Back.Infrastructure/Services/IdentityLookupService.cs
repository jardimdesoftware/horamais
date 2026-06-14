using Back.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services
{
    public class IdentityLookupService : IIdentityLookupService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public IdentityLookupService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityUser?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
