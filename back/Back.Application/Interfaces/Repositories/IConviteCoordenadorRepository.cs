using Back.Domain.Entities.Convite;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface IConviteCoordenadorRepository
{
    Task AddAsync(ConviteCoordenador convite);
    Task<ConviteCoordenador?> GetValidByTokenAsync(string token);
    Task<bool> TryConsumeAsync(System.Guid id);
    Task SaveChangesAsync();
}
