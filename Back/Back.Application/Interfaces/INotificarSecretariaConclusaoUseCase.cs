using Back.Domain.Entities.Atividade;
using System;
using System.Threading.Tasks;

namespace Back.Application.Interfaces;

public interface INotificarSecretariaConclusaoUseCase
{
    Task ExecuteAsync(Guid alunoId, TipoAtividade tipo);
}
