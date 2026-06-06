using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Turma;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class CreateTurmaUseCase
{
    private readonly ITurmaRepository _repo;

    public CreateTurmaUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<TurmaResponse> ExecuteAsync(CreateTurmaRequest request)
    {
        var periodo = TurmaInputValidator.ValidarPeriodo(request.Periodo);
        var turno = TurmaInputValidator.NormalizarTurno(request.Turno);

        if (await _repo.ExistsByCursoPeriodoTurnoAsync(request.CursoId, periodo, turno))
            throw new InvalidOperationException("Já existe uma turma para este curso, período e turno.");

        string codigo;
        bool exists;
        do
        {
            codigo = GenerateTurmaCode();
            exists = await _repo.ExistsByCodigoAsync(codigo);
        } while (exists);

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo(periodo)
            .WithTurno(turno)
            .WithCodigo(codigo)
            .WithPossuiExtensao(request.PossuiExtensao)
            .WithMaximoHorasExtensao(request.MaximoHorasExtensao)
            .WithCursoId(request.CursoId)
            .Build();

        await _repo.AddAsync(turma);

        // Recarrega com curso incluído
        var turmaCompleta = await _repo.GetByIdAsync(turma.Id);

        return new TurmaResponse(
            turmaCompleta!.Id,
            turmaCompleta.Periodo!,
            turmaCompleta.Turno!,
            turmaCompleta.Codigo!,
            turmaCompleta.CodigoAtivo,
            turmaCompleta.PossuiExtensao,
            turmaCompleta.MaximoHorasExtensao,
            turmaCompleta.CursoId,
            turmaCompleta.Curso?.Nome ?? "Curso não encontrado",
            turmaCompleta.Alunos.Count
        );
    }

    private static string GenerateTurmaCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        using var rng = RandomNumberGenerator.Create();
        var result = new char[6];
        var bytes = new byte[6];
        rng.GetBytes(bytes);
        for (int i = 0; i < 6; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        return new string(result);
    }
}
