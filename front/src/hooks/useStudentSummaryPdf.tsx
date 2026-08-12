import { useCallback, useState } from 'react';

import type {
  StudentSummaryActivity,
  StudentSummaryData
} from '@/components/StudentSummaryDocument';

import { TURNO_LABELS } from '@/config/constants';
import {
  listarCertificados,
  StatusCertificado
} from '@/services/certificateService';
import type {
  AlunoPorTurmaDetalhadoResponse,
  TurmaResponse
} from '@/services/classService';
import type { CoordenadorInfoResponse } from '@/services/coordinatorService';
import {
  obterAlunoDetalhado,
  type AtividadeAlunoResumo
} from '@/services/studentService';
import { saveAs } from 'file-saver';

interface GerarResumoParams {
  student: AlunoPorTurmaDetalhadoResponse;
  turma: TurmaResponse;
  coordenador: CoordenadorInfoResponse | null;
}

const DATE_FORMAT: Intl.DateTimeFormatOptions = {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric'
};

// As datas do certificado chegam do backend como meia-noite UTC e representam
// um dia de calendário: formatá-las no fuso local adiantaria/atrasaria o dia.
const UTC_DATE_FORMAT: Intl.DateTimeFormatOptions = {
  ...DATE_FORMAT,
  timeZone: 'UTC'
};

// O backend serializa o tipo da atividade ora como nome do enum, ora como o
// valor numérico — mesma normalização usada no painel do aluno.
const tipoDaAtividade = (atividade: AtividadeAlunoResumo) =>
  String(atividade.tipo ?? '').toUpperCase();

const isComplementar = (tipo: string) =>
  tipo === 'COMPLEMENTAR' || tipo === '1';

const isExtensao = (tipo: string) => tipo === 'EXTENSAO' || tipo === '0';

const formatarData = (data: string) =>
  new Date(data).toLocaleDateString('pt-BR', UTC_DATE_FORMAT);

const formatarPeriodo = (inicio: string, fim: string) => {
  const inicioFmt = formatarData(inicio);
  const fimFmt = formatarData(fim);
  return inicioFmt === fimFmt ? inicioFmt : `${inicioFmt} a ${fimFmt}`;
};

const toActivity = (
  atividade: AtividadeAlunoResumo
): StudentSummaryActivity => ({
  nome: atividade.nome,
  grupo: atividade.grupo,
  categoria: atividade.categoria,
  horasConcluidas: atividade.horasConcluidas,
  cargaMaximaCurso: atividade.cargaMaximaCurso
});

const nomeDoArquivo = (nome: string) =>
  `resumo_horas_${nome
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-zA-Z0-9]+/g, '_')
    .replace(/^_|_$/g, '')
    .toLowerCase()}.pdf`;

/**
 * Gera o resumo das informações do aluno em PDF, para repasse à secretaria.
 * O template e o renderizador são carregados sob demanda, fora do bundle da página.
 */
export function useStudentSummaryPdf() {
  const [isGenerating, setIsGenerating] = useState(false);

  const gerarResumo = useCallback(
    async ({ student, turma, coordenador }: GerarResumoParams) => {
      setIsGenerating(true);
      try {
        const [detalhado, certificados] = await Promise.all([
          obterAlunoDetalhado(student.id),
          listarCertificados(StatusCertificado.APROVADO, student.id)
        ]);

        const atividadesComHoras = detalhado.atividades.filter(
          (atividade) => atividade.horasConcluidas > 0
        );

        const data: StudentSummaryData = {
          aluno: {
            nome: student.nome,
            matricula: student.matricula,
            email: student.email,
            situacao: student.isAtivo ? 'Ativo' : 'Inativo'
          },
          turma: {
            curso: turma.cursoNome,
            codigo: turma.codigo,
            periodo: turma.periodo,
            turno: TURNO_LABELS[turma.turno] ?? turma.turno
          },
          horas: {
            complementarConcluidas: student.totalHorasComplementar,
            complementarExigidas: student.maximoHorasComplementar,
            extensaoConcluidas: student.totalHorasExtensao,
            extensaoExigidas: student.maximoHorasExtensao,
            possuiExtensao: turma.possuiExtensao
          },
          atividadesComplementares: atividadesComHoras
            .filter((atividade) => isComplementar(tipoDaAtividade(atividade)))
            .map(toActivity),
          atividadesExtensao: atividadesComHoras
            .filter((atividade) => isExtensao(tipoDaAtividade(atividade)))
            .map(toActivity),
          certificados: certificados.map((certificado) => ({
            titulo: certificado.tituloAtividade,
            instituicao: certificado.instituicao || certificado.local,
            categoria: certificado.categoria,
            periodoLetivo: certificado.periodoLetivo,
            periodo: formatarPeriodo(
              certificado.dataInicio,
              certificado.dataFim
            ),
            cargaHoraria: certificado.cargaHoraria
          })),
          coordenador,
          emitidoEm: new Date().toLocaleDateString('pt-BR', DATE_FORMAT)
        };

        const [{ pdf }, { StudentSummaryDocument }] = await Promise.all([
          import('@react-pdf/renderer'),
          import('@/components/StudentSummaryDocument')
        ]);

        const blob = await pdf(<StudentSummaryDocument data={data} />).toBlob();
        saveAs(blob, nomeDoArquivo(student.nome));
      } finally {
        setIsGenerating(false);
      }
    },
    []
  );

  return { gerarResumo, isGenerating };
}
