'use client';

import { useSession } from 'next-auth/react';
import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import {
  FaSearch,
  FaCheckCircle,
  FaTimesCircle,
  FaHourglassHalf,
  FaRegFileAlt
} from 'react-icons/fa';
import { toast } from 'react-toastify';

import { CertificateDetailsCard } from '@/components/CertificateDetailsCard';
import LoadingOverlay from '@/components/LoadingOverlay';
import { BreadcrumbAuto } from '@/components/ui/breadcrumb';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog';

import { CERTIFICADOS_ATUALIZADOS_EVENT } from '@/hooks/useCertificadoNotificacoes';
import { useLoadingOverlay } from '@/hooks/useLoadingOverlay';
import { extractApiError } from '@/lib/apiError';
import {
  listarCertificadosPorCurso,
  baixarAnexoCertificado,
  StatusCertificado,
  CertificadoPorCursoResponse,
  aprovarCertificado,
  reprovarCertificado
} from '@/services/certificateService';

const useIsMobile = () => {
  const [mobile, setMobile] = useState(false);
  useEffect(() => {
    const resize = () => setMobile(window.innerWidth < 768);
    resize();
    window.addEventListener('resize', resize);
    return () => window.removeEventListener('resize', resize);
  }, []);
  return mobile;
};

const StatusIcon: React.FC<{ status: StatusCertificado }> = ({ status }) => {
  switch (status) {
    case 'APROVADO':
      return <FaCheckCircle className="text-green-500" title="Aprovado" />;
    case 'REPROVADO':
      return <FaTimesCircle className="text-red-500" title="Reprovado" />;
    case 'PENDENTE':
      return <FaHourglassHalf className="text-yellow-500" title="Pendente" />;
    default:
      return null;
  }
};

const formatarData = (inicio: string, fim: string): string => {
  const dataInicio = new Date(inicio);
  const dataFim = new Date(fim);

  const options: Intl.DateTimeFormatOptions = {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  };

  const inicioFmt = dataInicio.toLocaleDateString('pt-BR', options);
  const fimFmt = dataFim.toLocaleDateString('pt-BR', options);

  return inicioFmt === fimFmt ? inicioFmt : `${inicioFmt} a ${fimFmt}`;
};

interface RejectFormData {
  justificativa: string;
}

export default function ValidacaoCertificadosPage() {
  const isMobile = useIsMobile();
  const { data: session } = useSession();
  const cursoId = session?.user?.cursoId || '';

  const [certificados, setCertificados] = useState<
    CertificadoPorCursoResponse[]
  >([]);
  const [filtroStatus, setFiltroStatus] = useState<StatusCertificado | 'todos'>(
    StatusCertificado.PENDENTE
  );
  const [termoBusca, setTermoBusca] = useState('');
  const [certificadoSelecionado, setCertificadoSelecionado] =
    useState<CertificadoPorCursoResponse | null>(null);
  const [rejectModalOpen, setRejectModalOpen] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<RejectFormData>();

  const { visible, show, hide } = useLoadingOverlay();

  useEffect(() => {
    const fetchCertificados = async () => {
      try {
        if (!cursoId) return;
        show();
        const result = await listarCertificadosPorCurso(cursoId);
        setCertificados(result);
      } catch (error) {
      } finally {
        hide();
      }
    };
    fetchCertificados();
  }, [cursoId, show, hide]);

  const certificadosFiltrados = certificados
    .filter((c) => filtroStatus === 'todos' || c.status === filtroStatus)
    .filter(
      (c) =>
        c.alunoNome.toLowerCase().includes(termoBusca.toLowerCase()) ||
        c.tituloAtividade.toLowerCase().includes(termoBusca.toLowerCase()) ||
        c.categoria.toLowerCase().includes(termoBusca.toLowerCase())
    );

  const handleSelectCertificado = (c: CertificadoPorCursoResponse) =>
    setCertificadoSelecionado(c);

  const handleApprove = async (novaCargaHoraria?: number) => {
    if (!certificadoSelecionado) return;

    try {
      show();
      await aprovarCertificado(certificadoSelecionado.id, novaCargaHoraria);

      const cargaCorrigida =
        novaCargaHoraria !== undefined &&
        novaCargaHoraria !== certificadoSelecionado.cargaHoraria;

      setCertificados((prev) =>
        prev.map((c) =>
          c.id === certificadoSelecionado.id
            ? {
                ...c,
                status: StatusCertificado.APROVADO,
                ...(cargaCorrigida && {
                  cargaHorariaOriginal: c.cargaHoraria,
                  cargaHoraria: novaCargaHoraria!,
                  cargaHorariaCorrigida: true
                })
              }
            : c
        )
      );

      setCertificadoSelecionado({
        ...certificadoSelecionado,
        status: StatusCertificado.APROVADO,
        ...(cargaCorrigida && {
          cargaHorariaOriginal: certificadoSelecionado.cargaHoraria,
          cargaHoraria: novaCargaHoraria!,
          cargaHorariaCorrigida: true
        })
      });

      window.dispatchEvent(new Event(CERTIFICADOS_ATUALIZADOS_EVENT));
      toast.success('Certificado aprovado com sucesso!');
    } catch (error) {
      toast.error(
        extractApiError(
          error,
          'Erro ao aprovar certificado! Tente novamente mais tarde.'
        )
      );
    } finally {
      hide();
    }
  };

  const handleOpenRejectModal = () => {
    if (!certificadoSelecionado) return;
    reset();
    setRejectModalOpen(true);
  };

  const handleConfirmReject = async (data: RejectFormData) => {
    if (!certificadoSelecionado) return;

    try {
      show();
      await reprovarCertificado(certificadoSelecionado.id, data.justificativa);

      setCertificados((prev) =>
        prev.map((c) =>
          c.id === certificadoSelecionado.id
            ? {
                ...c,
                status: StatusCertificado.REPROVADO,
                justificativaRejeicao: data.justificativa
              }
            : c
        )
      );

      setCertificadoSelecionado({
        ...certificadoSelecionado,
        status: StatusCertificado.REPROVADO,
        justificativaRejeicao: data.justificativa
      });

      setRejectModalOpen(false);
      window.dispatchEvent(new Event(CERTIFICADOS_ATUALIZADOS_EVENT));
      toast.success('Certificado reprovado com sucesso!');
    } catch (error) {
      toast.error(
        extractApiError(
          error,
          'Erro ao reprovar certificado! Tente novamente mais tarde.'
        )
      );
    } finally {
      hide();
    }
  };

  const handleViewPdf = async (id: string) => {
    try {
      const blob = await baixarAnexoCertificado(id);
      const url = window.URL.createObjectURL(blob);
      window.open(url, '_blank');
    } catch (error) {
      toast.error('Não foi possível visualizar o PDF. Tente novamente.');
    }
  };

  const showDetailMobile = isMobile && certificadoSelecionado;

  return (
    <div className="flex flex-col h-screen">
      <LoadingOverlay show={visible} />

      <div className="p-6 bg-gray-50">
        <BreadcrumbAuto />
        <div className="mt-4 flex flex-col sm:flex-row gap-4 items-center">
          <div className="relative flex-grow w-full sm:w-auto">
            <input
              type="text"
              placeholder="Buscar por aluno, atividade, categoria..."
              className="w-full p-3 pl-10 border text-black border-black rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={termoBusca}
              onChange={(e) => setTermoBusca(e.target.value)}
            />
            <FaSearch className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          </div>
          <div className="w-full sm:w-auto">
            <select
              className="w-full p-2 rounded-lg border border-black text-black shadow-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={filtroStatus}
              onChange={(e) =>
                setFiltroStatus(e.target.value as StatusCertificado | 'todos')
              }
            >
              <option value="todos">Todos Status</option>
              <option value={StatusCertificado.PENDENTE}>Pendentes</option>
              <option value={StatusCertificado.APROVADO}>Aprovados</option>
              <option value={StatusCertificado.REPROVADO}>Reprovados</option>
            </select>
          </div>
        </div>
      </div>

      <div className="flex-grow flex overflow-hidden">
        <div
          className={`w-full md:w-3/5 lg:w-2/3 p-6 overflow-y-auto ${showDetailMobile ? 'hidden' : ''}`}
        >
          {certificadosFiltrados.length ? (
            <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead
                  className="text-left text-gray-700 text-xs uppercase tracking-wide"
                  style={{
                    backgroundColor: '#F2F2F2',
                    borderBottom: '1px solid #D1D1D1'
                  }}
                >
                  <tr>
                    {[
                      'Turma',
                      'Categoria',
                      'Atividade',
                      'Horas',
                      'Aluno',
                      'Status'
                    ].map((h) => (
                      <th key={h} className="px-4 py-3">
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {certificadosFiltrados.map((c) => (
                    <tr
                      key={c.id}
                      onClick={() => handleSelectCertificado(c)}
                      className={`border-b last:border-none cursor-pointer transition-colors ${certificadoSelecionado?.id === c.id ? 'bg-blue-50' : 'hover:bg-gray-50'}`}
                    >
                      <td className="px-4 py-3 text-gray-700">
                        {c.periodoTurma}
                      </td>
                      <td className="px-4 py-3">
                        <span
                          className={`px-2 inline-flex text-xs font-semibold rounded-full ${
                            c.tipo === 'EXTENSAO'
                              ? 'bg-green-100 text-green-800'
                              : 'bg-blue-100 text-blue-800'
                          }`}
                        >
                          {c.categoria}
                        </span>
                      </td>
                      <td
                        className="px-4 py-3 text-gray-700 truncate max-w-xs"
                        title={c.tituloAtividade}
                      >
                        {c.tituloAtividade}
                      </td>
                      <td className="px-4 py-3 text-gray-700">
                        {c.cargaHoraria}h
                      </td>
                      <td className="px-4 py-3 text-gray-700">{c.alunoNome}</td>
                      <td className="px-4 py-3">
                        <StatusIcon status={c.status as StatusCertificado} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="text-center py-10">
              <p className="text-gray-500">
                Nenhum certificado encontrado para os filtros aplicados.
              </p>
            </div>
          )}
        </div>

        {(certificadoSelecionado || !isMobile) && (
          <aside
            className={
              isMobile
                ? 'fixed inset-0 z-40 bg-gray-50 p-6 overflow-y-auto'
                : 'w-full md:w-2/5 lg:w-1/3 p-6 bg-gray-50 border-l border-gray-200 overflow-y-auto'
            }
          >
            {certificadoSelecionado ? (
              <CertificateDetailsCard
                key={certificadoSelecionado.id}
                name={certificadoSelecionado.alunoNome}
                registration={certificadoSelecionado.alunoMatricula}
                email={certificadoSelecionado.alunoEmail}
                activity={certificadoSelecionado.tituloAtividade}
                category={certificadoSelecionado.categoria}
                description={certificadoSelecionado.tituloAtividade}
                location={certificadoSelecionado.local}
                date={formatarData(
                  certificadoSelecionado.dataInicio,
                  certificadoSelecionado.dataFim
                )}
                workload={`${certificadoSelecionado.cargaHoraria} horas`}
                workloadValue={certificadoSelecionado.cargaHoraria}
                {...(certificadoSelecionado?.status ===
                StatusCertificado.PENDENTE
                  ? { onApprove: handleApprove }
                  : {})}
                {...(certificadoSelecionado?.status ===
                StatusCertificado.PENDENTE
                  ? { onReject: handleOpenRejectModal }
                  : {})}
                onViewPdf={() => handleViewPdf(certificadoSelecionado.id)}
                {...(isMobile
                  ? { onBack: () => setCertificadoSelecionado(null) }
                  : {})}
              />
            ) : (
              !isMobile && (
                <div className="flex flex-col items-center justify-center text-center text-gray-500 p-8 h-full">
                  <FaRegFileAlt className="text-5xl mb-4 text-gray-400" />
                  <p className="font-semibold text-lg">
                    Nenhum certificado selecionado
                  </p>
                  <p className="text-sm mt-1">
                    Clique em um certificado na lista para visualizar os
                    detalhes e realizar ações.
                  </p>
                </div>
              )
            )}
          </aside>
        )}
      </div>

      <Dialog open={rejectModalOpen} onOpenChange={setRejectModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reprovar Certificado</DialogTitle>
            <DialogDescription>
              Informe o motivo da reprovação. O aluno poderá visualizar esta
              justificativa.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit(handleConfirmReject)}>
            <div className="py-4">
              <textarea
                id="justificativa-rejeicao"
                className={`w-full min-h-[120px] p-3 border rounded-lg resize-y text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-red-500 ${
                  errors.justificativa ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="Descreva o motivo da reprovação..."
                {...register('justificativa', {
                  required: 'A justificativa é obrigatória.',
                  minLength: {
                    value: 10,
                    message: 'A justificativa deve ter no mínimo 10 caracteres.'
                  }
                })}
              />
              {errors.justificativa && (
                <p className="text-red-500 text-xs mt-1">
                  {errors.justificativa.message}
                </p>
              )}
            </div>

            <DialogFooter>
              <button
                type="button"
                onClick={() => setRejectModalOpen(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition cursor-pointer"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition disabled:opacity-50 cursor-pointer"
              >
                {isSubmitting ? 'Reprovando...' : 'Confirmar Reprovação'}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
