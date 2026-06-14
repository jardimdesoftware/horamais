using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Certificado;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface ICertificadoRepository
{
    Task AddAsync(Certificado certificado);
    Task<IEnumerable<Certificado>> GetAsync(StatusCertificado? status, Guid? alunoId);
    Task<Certificado?> GetByIdAsync(Guid id);
    Task UpdateAsync(Certificado certificado);
    Task<IEnumerable<Certificado>> GetByAlunoAtividadeAsync(Guid alunoAtividadeId);
    Task<Certificado?> GetByIdWithAlunoAtividadeAsync(Guid id);
    Task<IEnumerable<Certificado>> GetAllWithAlunoAtividadeAsync();
    Task<string?> GetStorageKeyByIdAsync(Guid certificadoId);
    Task<IEnumerable<Certificado>> GetCertificadosAprovadosPorAlunoAsync(Guid alunoId);
    Task DeleteAsync(Certificado certificado);
    Task RemoveRangeAsync(IEnumerable<Certificado> certificados);
    Task<IEnumerable<Certificado>> GetByAlunoAtividadeIdTrackedAsync(Guid alunoAtividadeId);
    Task<IEnumerable<Certificado>> GetByAlunoAtividadeAndStatusAsync(Guid alunoAtividadeId, IEnumerable<StatusCertificado> statuses);
}
