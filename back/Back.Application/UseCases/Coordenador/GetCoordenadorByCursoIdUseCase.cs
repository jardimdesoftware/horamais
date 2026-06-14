using System.Threading.Tasks;
using System;
using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces.Repositories;

public class GetCoordenadorByCursoIdUseCase
{
    private readonly ICoordenadorRepository _repo;

    public GetCoordenadorByCursoIdUseCase(ICoordenadorRepository repo)
    {
        _repo = repo;
    }

    public async Task<CoordenadorResumoResponse?> ExecuteAsync(Guid cursoId)
    {
        var coordenador = await _repo.GetByCursoIdAsync(cursoId);

        if (coordenador == null)
            return null;

        return new CoordenadorResumoResponse
        {
            Id = coordenador.Id,
            Nome = coordenador.Nome!
        };
    }
}
