using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class ResetarCodigoUseCase
{
    private readonly ITurmaRepository _repo;

    public ResetarCodigoUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<TurmaResponse> ExecuteAsync(string identifier)
    {
        var turma = await _repo.GetByIdentifierTrackedAsync(identifier);
        if (turma == null)
            throw new KeyNotFoundException("Turma não encontrada.");

        string novoCodigo;
        bool exists;
        do
        {
            novoCodigo = GerarCodigo();
            exists = await _repo.ExistsByCodigoAsync(novoCodigo);
        } while (exists);

        turma.Codigo = novoCodigo;
        turma.CodigoAtivo = true;
        await _repo.UpdateAsync(turma);

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

    private static string GerarCodigo()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        using var rng = RandomNumberGenerator.Create();
        var result = new char[6];
        var bytes = new byte[6];
        rng.GetBytes(bytes);
        for (int i = 0; i < 6; i++)
            result[i] = chars[bytes[i] % chars.Length];
        return new string(result);
    }
}
