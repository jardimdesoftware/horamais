using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Back.Application.Interfaces.Identity
{
    public interface IIdentityLookupService
    {
        Task<IdentityUser?> GetByEmailAsync(string email);
    }
}
