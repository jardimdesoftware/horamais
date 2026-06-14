using Back.Application.DTOs.Campus;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Campus;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Campus;

public class CriarCampusUseCase
{
    private readonly ICampusRepository _repository;

    public CriarCampusUseCase(ICampusRepository repository)
    {
        _repository = repository;
    }

    public async Task<CampusResponse> ExecuteAsync(CreateCampusRequest request)
    {
        var campus = new CampusBuilder()
            .WithId(Guid.NewGuid())
            .WithNome(request.Nome!)
            .WithCidade(request.Cidade!)
            .Build();

        await _repository.AddAsync(campus);

        return new CampusResponse(campus.Id, campus.Nome!, campus.Cidade!);
    }
}
