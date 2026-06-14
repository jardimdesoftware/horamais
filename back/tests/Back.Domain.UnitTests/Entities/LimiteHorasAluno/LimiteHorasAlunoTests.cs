using System;
using System.Linq;
using Back.Domain.Entities.LimiteHorasAluno;
using FluentAssertions;
using Xunit;
using Back.Domain.UnitTests;

namespace Back.Domain.UnitTests;

public class LimiteHorasAlunoTests
{
    [Fact]
    public void Limite_Valido_Deve_Passar()
    {
        var limite = new LimiteHorasAlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithMaximoHorasComplementar(40)
            .WithCursoId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(limite);
        results.Should().BeEmpty();
    }

    [Fact]
    public void Limite_Sem_MaximoHorasComplementar_Deve_Passar_Porque_Int_Required_Nao_Valida()
    {
        var limite = new LimiteHorasAlunoBuilder()
            .WithId(Guid.NewGuid())
            // NÃO preenchendo MaximoHorasComplementar → valor default = 0 → DataAnnotations considera válido
            .WithCursoId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(limite);

        // DataAnnotations NÃO acusa erro em int default (0)
        results.Should().BeEmpty();
    }

    [Fact]
    public void Limite_Sem_CursoId_Deve_Passar_Porque_Guid_Default_Eh_Valido()
    {
        var limite = new LimiteHorasAlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithMaximoHorasComplementar(20)
            .Build();

        var results = ValidationHelper.ValidateObject(limite);

        // Guid default é considerado válido pelo DataAnnotations
        results.Should().BeEmpty();
    }

}
