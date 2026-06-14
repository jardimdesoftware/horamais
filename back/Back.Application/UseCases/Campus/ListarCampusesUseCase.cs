using Back.Application.DTOs.Campus;
using Back.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Campus;

public class ListarCampusesUseCase
{
    private readonly ICampusRepository _repository;

    public ListarCampusesUseCase(ICampusRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CampusResponse>> ExecuteAsync()
    {
        var campi = await _repository.GetAllAsync();
        return campi.Select(c => new CampusResponse(c.Id, c.Nome!, c.Cidade!));
    }
}
