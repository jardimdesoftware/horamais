using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Back.Domain.Entities.Atividade;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Back.Infrastructure.Seeders;

public static class AtividadeSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Atividades.AnyAsync())
            return;

        var atividades = new List<Atividade>
        {
            // ── Atividades Complementares (Quadro 15) ──────────────────────────

            // Categoria I
            new Atividade { Id = Guid.NewGuid(), Nome = "Disciplinas cursadas em outros cursos de graduação", Grupo = "I", Categoria = "Categoria I", CategoriaKey = "categoria-i", CargaMaximaSemestral = 60, CargaMaximaCurso = 60, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Monitoria", Grupo = "I", Categoria = "Categoria I", CategoriaKey = "categoria-i", CargaMaximaSemestral = 40, CargaMaximaCurso = 80, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Cursos de idiomas realizados durante o curso, comunicação e expressão e informática", Grupo = "I", Categoria = "Categoria I", CategoriaKey = "categoria-i", CargaMaximaSemestral = 40, CargaMaximaCurso = 80, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Cursos extras realizados durante o curso", Grupo = "I", Categoria = "Categoria I", CategoriaKey = "categoria-i", CargaMaximaSemestral = 20, CargaMaximaCurso = 40, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Participar do Programa Institucional de Bolsas de Iniciação à Docência - PIBID", Grupo = "I", Categoria = "Categoria I", CategoriaKey = "categoria-i", CargaMaximaSemestral = 40, CargaMaximaCurso = 80, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Atividades em laboratório diferentes daquelas desenvolvidas durante o horário regular do curso", Grupo = "I", Categoria = "Categoria I", CategoriaKey = "categoria-i", CargaMaximaSemestral = 80, CargaMaximaCurso = 80, Tipo = TipoAtividade.COMPLEMENTAR },

            // Categoria II
            new Atividade { Id = Guid.NewGuid(), Nome = "Estágio Profissional não obrigatório", Grupo = "II", Categoria = "Categoria II", CategoriaKey = "categoria-ii", CargaMaximaSemestral = 60, CargaMaximaCurso = 60, Tipo = TipoAtividade.COMPLEMENTAR },

            // Categoria III
            new Atividade { Id = Guid.NewGuid(), Nome = "Participação como ouvinte, participante, palestrante, instrutor, apresentador, expositor ou mediador em eventos científicos, seminários, atividades culturais, esportivas, políticas e sociais, sessões técnicas, exposições, jornadas acadêmicas e científicas, palestras, congressos, visitas técnicas, conferências ou similares", Grupo = "III", Categoria = "Categoria III", CategoriaKey = "categoria-iii", CargaMaximaSemestral = 10, CargaMaximaCurso = 80, Tipo = TipoAtividade.COMPLEMENTAR },

            // Categoria IV
            new Atividade { Id = Guid.NewGuid(), Nome = "Participação em projetos de pesquisa aprovados pelo IFPE", Grupo = "IV", Categoria = "Categoria IV", CategoriaKey = "categoria-iv", CargaMaximaSemestral = 20, CargaMaximaCurso = 80, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Publicações de textos acadêmicos", Grupo = "IV", Categoria = "Categoria IV", CategoriaKey = "categoria-iv", CargaMaximaSemestral = 10, CargaMaximaCurso = 40, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Grupos de estudos com produção intelectual", Grupo = "IV", Categoria = "Categoria IV", CategoriaKey = "categoria-iv", CargaMaximaSemestral = 10, CargaMaximaCurso = 20, Tipo = TipoAtividade.COMPLEMENTAR },
            new Atividade { Id = Guid.NewGuid(), Nome = "Trabalhos desenvolvidos sob orientação de docente apresentados em eventos acadêmicos", Grupo = "IV", Categoria = "Categoria IV", CategoriaKey = "categoria-iv", CargaMaximaSemestral = 10, CargaMaximaCurso = 40, Tipo = TipoAtividade.COMPLEMENTAR },

            // ── Práticas Curriculares de Extensão (Quadro 14) ─────────────────

            new Atividade { Id = Guid.NewGuid(), Nome = "Participação como Bolsista, voluntário ou colaborador, em Projetos/Programas de Extensão coordenado por servidor do IFPE", Grupo = "Extensão", Categoria = "Extensão", CategoriaKey = "extensao", CargaMaximaSemestral = 80, CargaMaximaCurso = 160, Tipo = TipoAtividade.EXTENSAO },
            new Atividade { Id = Guid.NewGuid(), Nome = "Participação em Eventos voltados ao público externo como Palestrante, Instrutor, Apresentador, Expositor ou Mediador de Cursos/Palestras/Workshops/Mesas Redondas/Oficinas", Grupo = "Extensão", Categoria = "Extensão", CategoriaKey = "extensao", CargaMaximaSemestral = 40, CargaMaximaCurso = 80, Tipo = TipoAtividade.EXTENSAO },
            new Atividade { Id = Guid.NewGuid(), Nome = "Participação da Comissão Organizadora ou como Monitor em Eventos para exibição pública de conhecimento ou produto (Cultural, Acadêmico, Científico ou Tecnológico) desenvolvido no IFPE", Grupo = "Extensão", Categoria = "Extensão", CategoriaKey = "extensao", CargaMaximaSemestral = 20, CargaMaximaCurso = 80, Tipo = TipoAtividade.EXTENSAO },
            new Atividade { Id = Guid.NewGuid(), Nome = "Prestação de serviços como voluntário/a, bolsista ou colaborador/a, para o desenvolvimento de produtos e/ou processos voltados à resolução de problemas identificados interna ou externamente ao IFPE", Grupo = "Extensão", Categoria = "Extensão", CategoriaKey = "extensao", CargaMaximaSemestral = 80, CargaMaximaCurso = 160, Tipo = TipoAtividade.EXTENSAO },
            new Atividade { Id = Guid.NewGuid(), Nome = "Atividades desenvolvidas por meio dos Núcleos Institucionais de Extensão", Grupo = "Extensão", Categoria = "Extensão", CategoriaKey = "extensao", CargaMaximaSemestral = 20, CargaMaximaCurso = 80, Tipo = TipoAtividade.EXTENSAO },
            new Atividade { Id = Guid.NewGuid(), Nome = "Atividades de empreendedorismo, como membro de empresa júnior ou como voluntário/a ou bolsista de incubadoras de empresa ou projetos, prestando assessoria e consultoria", Grupo = "Extensão", Categoria = "Extensão", CategoriaKey = "extensao", CargaMaximaSemestral = 80, CargaMaximaCurso = 160, Tipo = TipoAtividade.EXTENSAO }
        };

        context.Atividades.AddRange(atividades);
        await context.SaveChangesAsync();
    }
}
