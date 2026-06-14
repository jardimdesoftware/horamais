using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Application.DTOs.Coordenador;
public record CoordenadorInfoResponse(
    string Nome,
    string Curso,
    string NumeroPortaria,
    string DOU
);