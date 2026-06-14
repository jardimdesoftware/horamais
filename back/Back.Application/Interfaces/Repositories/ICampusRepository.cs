using Back.Domain.Entities.Campus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface ICampusRepository
{
    Task AddAsync(Campus campus);
    Task<IEnumerable<Campus>> GetAllAsync();
    Task<Campus?> GetByIdAsync(Guid id);
}
