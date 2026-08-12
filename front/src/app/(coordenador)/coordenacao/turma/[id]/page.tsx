'use client';

import { useParams } from 'next/navigation';
import { useEffect, useState } from 'react';
import { FaCopy, FaCheck, FaUser } from 'react-icons/fa';
import { toast } from 'react-toastify';

import { StudentCard } from './_components/StudentCard';
import LoadingOverlay from '@/components/LoadingOverlay';
import { Badge } from '@/components/ui/badge';
import { BreadcrumbAuto } from '@/components/ui/breadcrumb';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card';

import { TURNO_LABELS } from '@/config/constants';
import { useStudentSummaryPdf } from '@/hooks/useStudentSummaryPdf';
import {
  useAlunosPorTurma,
  useTurma,
  turmaAlunosKey
} from '@/hooks/useTurmaAlunos';
import { useTurmaRealtime } from '@/hooks/useTurmaRealtime';
import { extractApiError } from '@/lib/apiError';
import {
  obterCoordenadorAutenticado,
  type CoordenadorInfoResponse
} from '@/services/coordinatorService';
import {
  toggleStatusAluno,
  listarConcluidosComplementar,
  listarConcluidosExtensao,
  marcarDownloadComplementar,
  marcarDownloadExtensao
} from '@/services/studentService';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import Docxtemplater from 'docxtemplater';
import { saveAs } from 'file-saver';
import PizZip from 'pizzip';

const VisualizarTurma = () => {
  const { id } = useParams();
  const turmaId = id as string;
  const queryClient = useQueryClient();

  const [isDownloading, setIsDownloading] = useState(false);
  const [copied, setCopied] = useState(false);

  const { gerarResumo, isGenerating: isGeneratingSummary } =
    useStudentSummaryPdf();

  const { data: turma, isLoading: isLoadingTurma } = useTurma(turmaId);
  const {
    data: students = [],
    isLoading: isLoadingAlunos,
    isError: isErrorAlunos
  } = useAlunosPorTurma(turmaId);
  const { data: coordenador = null } = useQuery<CoordenadorInfoResponse>({
    queryKey: ['coordenador-autenticado'],
    queryFn: obterCoordenadorAutenticado
  });

  // Tempo real: novos alunos da turma aparecem ao vivo, sem recarregar a página.
  // O grupo SignalR usa o GUID da turma (turma.id) para casar com o backend; a
  // invalidação usa o identificador da URL (turmaId), igual à queryKey da lista.
  useTurmaRealtime(turma?.id, turmaId);

  useEffect(() => {
    if (isErrorAlunos) toast.error('Erro ao carregar dados da turma.');
  }, [isErrorAlunos]);

  const toggleStatusMutation = useMutation({
    mutationFn: toggleStatusAluno,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: turmaAlunosKey(turmaId) });
    }
  });

  const copyCode = async () => {
    if (!turma) return;
    navigator.clipboard.writeText(turma.codigo);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const toggleStudentStatus = async (studentId: string) => {
    const student = students.find((s) => s.id === studentId);
    if (!student) return;
    try {
      await toggleStatusMutation.mutateAsync(studentId);
      toast.info(
        `${student.nome} foi ${student.isAtivo ? 'desativado' : 'ativado'}.`
      );
    } catch (error) {
      toast.error(extractApiError(error, 'Não foi possível alterar o status.'));
    }
  };

  const handleDownload = async (
    studentId: string,
    categoria: 'complementar' | 'extensao'
  ) => {
    if (!coordenador) return;

    setIsDownloading(true);
    try {
      const concluidos =
        categoria === 'complementar'
          ? await listarConcluidosComplementar()
          : await listarConcluidosExtensao();

      const aluno = concluidos.find((a) => a.id === studentId);
      if (!aluno) {
        toast.error(
          'Dados do aluno não encontrados para geração do relatório.'
        );
        return;
      }

      const response = await fetch('/docs/Coordenador-Requerimento.docx');
      const templateBuffer = await response.arrayBuffer();

      const safeString = (v: string | null | undefined) => v ?? '';

      const certs = aluno.certificados.map((cert, idx) => ({
        idx: idx + 1,
        tituloAtividade: safeString(cert.titulo),
        titulo: safeString(cert.titulo),
        instituicao: safeString(cert.instituicao) || safeString(cert.local),
        localRealizacao: safeString(cert.local),
        categoria: safeString(cert.categoria),
        periodoLetivo: safeString(cert.periodoLetivo),
        periodoLetivoFaculdade: safeString(cert.periodoLetivo),
        cargaHoraria: cert.cargaHoraria || 0,
        dataInicioAtividade: safeString(cert.periodoInicio),
        dataFimAtividade: safeString(cert.periodoFim),
        dataInicio: safeString(cert.periodoInicio),
        dataFim: safeString(cert.periodoFim),
        totalPeriodos: cert.totalPeriodos || 1,
        especificacaoAtividade: safeString(cert.descricao),
        especificacao: safeString(cert.descricao),
        title: safeString(cert.titulo),
        periodo:
          cert.periodoInicio && cert.periodoFim
            ? `${safeString(cert.periodoInicio)} a ${safeString(cert.periodoFim)}`
            : safeString(cert.periodoInicio),
        descricao: safeString(cert.descricao)
      }));

      const docxVars = {
        Coordenador: coordenador.nome,
        curso: coordenador.curso,
        Portaria: coordenador.numeroPortaria,
        DOU: coordenador.dou,
        alunos: [
          {
            estudante: aluno.nome,
            matricula: aluno.matricula,
            carga: aluno.cargaHoraria
          }
        ],
        certs
      };

      const zip = new PizZip(templateBuffer);
      const doc = new Docxtemplater(zip, {
        paragraphLoop: true,
        linebreaks: true
      });
      doc.setData(docxVars);
      doc.render();

      const out = doc.getZip().generate({ type: 'blob' });
      saveAs(out, `contabilizacao_${aluno.nome?.replaceAll(' ', '_')}.docx`);

      if (categoria === 'complementar') {
        await marcarDownloadComplementar(studentId);
      } else {
        await marcarDownloadExtensao(studentId);
      }

      toast.success(`Relatório de ${aluno.nome} gerado com sucesso.`);
    } catch (error) {
      toast.error(
        extractApiError(error, 'Não foi possível gerar o relatório.')
      );
    } finally {
      setIsDownloading(false);
    }
  };

  const handleGerarResumo = async (studentId: string) => {
    const student = students.find((s) => s.id === studentId);
    if (!student || !turma) return;

    try {
      await gerarResumo({ student, turma, coordenador });
      toast.success(`Resumo de ${student.nome} gerado com sucesso.`);
    } catch (error) {
      toast.error(
        extractApiError(error, 'Não foi possível gerar o resumo do aluno.')
      );
    }
  };

  return (
    <div className="space-y-8 p-4 md:p-6">
      <LoadingOverlay
        show={
          isLoadingTurma ||
          isLoadingAlunos ||
          toggleStatusMutation.isPending ||
          isDownloading ||
          isGeneratingSummary
        }
      />

      <BreadcrumbAuto />

      {turma && (
        <>
          <Card>
            <CardHeader>
              <CardTitle className="text-xl">{turma.cursoNome}</CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div className="p-4 bg-gray-50 rounded-lg">
                <p className="text-sm font-medium text-gray-600">
                  Código da Turma
                </p>
                <p className="text-lg font-bold text-gray-900">
                  {turma.codigo}
                </p>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={copyCode}
                  className="mt-3 flex items-center space-x-2 cursor-pointer"
                >
                  {copied ? (
                    <FaCheck className="w-4 h-4 text-green-600" />
                  ) : (
                    <FaCopy className="w-4 h-4" />
                  )}
                  <span>{copied ? 'Copiado!' : 'Copiar'}</span>
                </Button>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg">
                <p className="text-sm font-medium text-gray-600">Período</p>
                <p className="text-lg font-bold text-gray-900">
                  {turma.periodo}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg">
                <p className="text-sm font-medium text-gray-600">Turno</p>
                <p className="text-lg font-bold text-gray-900">
                  {TURNO_LABELS[turma.turno] ?? turma.turno}
                </p>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex justify-between items-center">
                <div className="flex items-center space-x-2">
                  <FaUser className="w-5 h-5" />
                  <CardTitle>Alunos Matriculados</CardTitle>
                </div>
                <Badge variant="secondary" className="text-sm">
                  {students.filter((s) => s.isAtivo).length} ativos
                </Badge>
              </div>
              <CardDescription>
                {students.length} alunos cadastrados
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {[...students]
                .sort((a, b) => {
                  if (a.isAtivo !== b.isAtivo) return a.isAtivo ? -1 : 1;
                  return a.nome.localeCompare(b.nome, 'pt-BR');
                })
                .map((student) => (
                  <StudentCard
                    key={student.id}
                    student={student}
                    turma={turma}
                    onToggleStatus={toggleStudentStatus}
                    onDownload={handleDownload}
                    onGenerateSummary={handleGerarResumo}
                    isDownloading={isDownloading}
                    isGeneratingSummary={isGeneratingSummary}
                  />
                ))}
            </CardContent>
          </Card>

          <div className="bg-blue-50 rounded-lg p-4 sm:p-6">
            <h3 className="font-semibold text-blue-900 mb-2">
              Código da Turma
            </h3>
            <p className="text-blue-700 text-sm mb-3">
              Compartilhe o código <strong>{turma.codigo}</strong> com os alunos
              para que eles possam se inscrever na turma.
            </p>
            <Button
              variant="outline"
              onClick={copyCode}
              className="flex items-center space-x-2 text-blue-700 border-blue-300 cursor-pointer"
            >
              {copied ? (
                <FaCheck className="w-4 h-4 text-green-600" />
              ) : (
                <FaCopy className="w-4 h-4" />
              )}
              <span>{copied ? 'Copiado!' : 'Copiar código da turma'}</span>
            </Button>
          </div>
        </>
      )}
    </div>
  );
};

export default VisualizarTurma;
