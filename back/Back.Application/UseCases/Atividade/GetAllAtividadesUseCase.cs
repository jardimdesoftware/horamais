using System.Collections.Generic;
using System.Threading.Tasks;
using Back.Application.DTOs.Atividade;
using Back.Application.Interfaces.Repositories;
using System.Linq;

namespace Back.Application.UseCases.Atividade;

public class GetAllAtividadesUseCase
{
    private readonly IAtividadeRepository _repo;

    public GetAllAtividadesUseCase(IAtividadeRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<AtividadeResponse>> ExecuteAsync()
    {
        var atividades = await _repo.GetAllAsync();

        return atividades.Select(a =>
            new AtividadeResponse(a.Id, a.Nome!, a.Tipo.ToString(), a.Grupo!, a.Categoria!, a.CategoriaKey!, a.CargaMaximaSemestral, a.CargaMaximaCurso, a.PossuiCurricularizacaoExtensao, a.HorasCurricularizacaoExtensao)
        );
    }
}
