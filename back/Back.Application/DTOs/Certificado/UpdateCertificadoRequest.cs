using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Certificado;
using Microsoft.AspNetCore.Http; 
using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Certificado
{
    public class UpdateCertificadoRequest
    {
        [Required]
        public string TituloAtividade { get; set; } = string.Empty;

        [Required]
        public string Instituicao { get; set; } = string.Empty;

        [Required]
        public string Local { get; set; } = string.Empty;

        [Required]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        public string Grupo { get; set; } = string.Empty;

        [Required]
        public string PeriodoLetivo { get; set; } = string.Empty;

        [Required]
        [Range(1, 999)]
        public int CargaHoraria { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [Required]
        public int TotalPeriodos { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public TipoAtividade Tipo { get; set; }

        /// <summary>
        /// Opcional: Envie um novo arquivo apenas se desejar substituir o anexo existente.
        /// </summary>
        public IFormFile? Anexo { get; set; }
    }
}