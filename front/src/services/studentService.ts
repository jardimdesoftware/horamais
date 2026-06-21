import api from './api';

export interface CreateAlunoRequest {
  nome: string;
  email: string;
  matricula: string;
  senha: string;
  turmaId?: string;
  turmaCodigo?: string;
}

export interface CreateAlunoResponse {
  id: string;
  nome: string;
  email: string;
}

export interface AlunoResponse {
  id: string;
  nome: string;
  email: string;
  matricula: string;
  role: string;
}

export interface AtividadeAlunoResumo {
  atividadeId: string;
  nome: string;
  grupo: string;
  categoria: string;
  categoriaKey: string;
  cargaMaximaSemestral: number;
  cargaMaximaCurso: number;
  horasConcluidas: number;
  tipo: string;
}

export interface AlunoDetalhadoResponse {
  id: string;
  nome: string;
  email: string;
  matricula: string;
  isAtivo: boolean;
  turmaId: string;
  atividades: AtividadeAlunoResumo[];
  totalHorasExtensao: number;
  maximoHorasExtensao: number;
  totalHorasComplementar: number;
  maximoHorasComplementar: number;
}

export interface AlunoResumoHorasResponse {
  id: string;
  nome: string;
  email: string;
  totalHorasExtensao: number;
  maximoHorasExtensao: number;
  totalHorasComplementar: number;
  maximoHorasComplementar: number;
  isAtivo: boolean;
}
export interface CertificadoConcluidoResponse {
  id: string;
  grupo: string | null;
  categoria: string | null;
  titulo: string | null;
  descricao: string | null;
  cargaHoraria: number;
  local: string | null;
  periodoInicio: string | null;
  periodoFim: string | null;
  status: string;
  tipo: string;
  instituicao: string | null;
  periodoLetivo: string | null;
  totalPeriodos: number;
}

export interface AlunoComHorasConcluidasResponse {
  id: string;
  nome: string | null;
  matricula: string | null;
  email: string | null;
  curso: string | null;
  certificados: CertificadoConcluidoResponse[];
  cargaHoraria: number;
  cargaHorariaFinalizada: boolean;
  jaFezDownload: boolean;
  categoria: string;
}
export interface ContagemPendenciaDownloadResponse {
  totalPendencias: number;
}
// ========== Requisições à API ==========

// Criar novo aluno
export const criarAluno = async (
  dados: CreateAlunoRequest
): Promise<CreateAlunoResponse> => {
  const response = await api.post<CreateAlunoResponse>('/Aluno', dados);
  return response.data;
};

// Obter aluno por ID
export const obterAlunoPorId = async (id: string): Promise<AlunoResponse> => {
  const response = await api.get<AlunoResponse>(`/Aluno/${id}`);
  return response.data;
};

// Deletar aluno
export const deletarAluno = async (id: string): Promise<void> => {
  await api.delete(`/Aluno/${id}`);
};

// Ativar ou desativar aluno
export const toggleStatusAluno = async (id: string): Promise<void> => {
  await api.patch(`/Aluno/${id}/toggle-status`);
};

// Obter aluno detalhado por ID
export const obterAlunoDetalhado = async (
  id: string
): Promise<AlunoDetalhadoResponse> => {
  const response = await api.get<AlunoDetalhadoResponse>(
    `/Aluno/${id}/detalhado`
  );
  return response.data;
};

// Obter aluno autenticado (meus dados)
export const obterMeusDadosDetalhados =
  async (): Promise<AlunoDetalhadoResponse> => {
    const response = await api.get<AlunoDetalhadoResponse>(
      '/Aluno/meu-detalhado'
    );
    return response.data;
  };

// Listar resumo de horas de todos os alunos
export const listarResumoHoras = async (): Promise<
  AlunoResumoHorasResponse[]
> => {
  const response = await api.get<AlunoResumoHorasResponse[]>(
    '/Aluno/resumo-horas'
  );
  return response.data;
};
// Atualizar aluno por ID
export const atualizarAluno = async (
  id: string,
  dados: Partial<CreateAlunoRequest>
): Promise<AlunoResponse> => {
  const response = await api.put<AlunoResponse>(`/Aluno/${id}`, dados);
  return response.data;
};

export const listarConcluidosComplementar = async (): Promise<
  AlunoComHorasConcluidasResponse[]
> => {
  const response = await api.get<AlunoComHorasConcluidasResponse[]>(
    '/Aluno/concluidos/complementar'
  );
  return response.data;
};

// Lista alunos com 100% das horas de extensão concluídas
export const listarConcluidosExtensao = async (): Promise<
  AlunoComHorasConcluidasResponse[]
> => {
  const response = await api.get<AlunoComHorasConcluidasResponse[]>(
    '/Aluno/concluidos/extensao'
  );
  return response.data;
};

// Conta total de pendências de download de certificados
export const contarPendenciasDownload =
  async (): Promise<ContagemPendenciaDownloadResponse> => {
    const response = await api.get<ContagemPendenciaDownloadResponse>(
      '/Aluno/pendencias-download/contagem'
    );
    return response.data;
  };

// Marca relatório de horas complementares como baixado
export const marcarDownloadComplementar = async (
  alunoId: string
): Promise<void> => {
  await api.patch(`/Aluno/${alunoId}/marcar-download/complementar`);
};

// Marca relatório de horas de extensão como baixado
export const marcarDownloadExtensao = async (
  alunoId: string
): Promise<void> => {
  await api.patch(`/Aluno/${alunoId}/marcar-download/extensao`);
};

export interface AlunoEmRiscoResponse {
  id: string;
  nome: string;
  matricula: string;
  turmaPeriodo: string;
  turmaCodigo: string;
  cursoNome: string;
  totalHorasComplementar: number;
  maximoHorasComplementar: number;
  porcentagemConclusao: number;
  periodosDecorridos: number;
  horasPorPeriodo: number;
  duracaoEmPeriodos: number;
}

export const listarAlunosEmRisco = async (
  percentualMaximo: number = 100
): Promise<AlunoEmRiscoResponse[]> => {
  const response = await api.get<AlunoEmRiscoResponse[]>(
    `/Aluno/risco?percentualMaximo=${percentualMaximo}`
  );
  return response.data;
};
