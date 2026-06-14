using Back.Domain.Entities.AlunoAtividade;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class AlunoAtividadeTests
{
    [Fact]
    public void AlunoAtividade_Valido_Deve_Passar()
    {
        var alunoAtividade = new AlunoAtividadeBuilder()
            .WithId(Guid.NewGuid())
            .WithAlunoId(Guid.NewGuid())
            .WithAtividadeId(Guid.NewGuid())
            .WithHorasConcluidas(10)
            .Build();

        var results = ValidationHelper.ValidateObject(alunoAtividade);

        results.Should().BeEmpty();
    }

    [Fact]
    public void AlunoAtividade_Sem_Definir_Ids_Deve_Passar_Porque_Guid_Default_Eh_Valido()
    {
        var alunoAtividade = new AlunoAtividadeBuilder()
            .WithId(Guid.NewGuid())
            .WithHorasConcluidas(5)
            .Build();

        var results = ValidationHelper.ValidateObject(alunoAtividade);

        // DataAnnotations não valida Guid.Empty
        results.Should().BeEmpty();
    }

    [Fact]
    public void AlunoAtividade_Com_Horas_Negativas_Deve_Passar_Porque_DataAnnotations_Nao_Valida_Numeros()
    {
        var alunoAtividade = new AlunoAtividadeBuilder()
            .WithId(Guid.NewGuid())
            .WithAlunoId(Guid.NewGuid())
            .WithAtividadeId(Guid.NewGuid())
            .WithHorasConcluidas(-10)
            .Build();

        var results = ValidationHelper.ValidateObject(alunoAtividade);

        // DataAnnotations não proíbe negativos
        results.Should().BeEmpty();
    }
}
