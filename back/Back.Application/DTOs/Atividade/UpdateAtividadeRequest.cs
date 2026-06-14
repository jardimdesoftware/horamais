using Back.Domain.Entities.Atividade;
using System.ComponentModel.DataAnnotations;
using System;

namespace Back.Application.DTOs.Atividade
{
    public class UpdateAtividadeRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Grupo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CategoriaKey { get; set; } = string.Empty;

        [Required]
        [Range(0, 999)]
        public int CargaMaximaSemestral { get; set; }

        [Required]
        [Range(0, 9999)]
        public int CargaMaximaCurso { get; set; }

        [Required]
        public TipoAtividade Tipo { get; set; }

        public bool PossuiCurricularizacaoExtensao { get; set; }

        public int? HorasCurricularizacaoExtensao { get; set; }
    }
}