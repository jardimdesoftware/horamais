import { StatusCertificado } from '@/types';

import api from './api';

export { StatusCertificado };

export enum TipoCertificado {
  EXTENSAO = 'EXTENSAO',
  COMPLEMENTAR = 'COMPLEMENTAR'
}

export interface CertificadoResponse {
  id: string;
  tituloAtividade: string;
  instituicao: string;
  local: string;
  categoria: string;
  grupo: string;
  periodoLetivo: string;
  cargaHoraria: number;
  dataInicio: string;
  dataFim: string;
  totalPeriodos: number;
  descricao?: string;
  tipo: TipoCertificado;
  status: StatusCertificado;
  alunoId: string;
  atividadeId: string;
  categoriaKey: string;
  justificativaRejeicao?: string;
  cargaHorariaOriginal?: number;
  cargaHorariaCorrigida: boolean;
}

export interface CertificadoDetalhadoResponse extends CertificadoResponse {}

export interface CreateCertificadoRequest {
  tituloAtividade: string;
  instituicao: string;
  local: string;
  categoria: string;
  grupo: string;
  periodoLetivo: string;
  cargaHoraria: number;
  dataInicio: string;
  dataFim: string;
  totalPeriodos: number;
  descricao?: string;
  anexo: File;
  alunoId: string;
  atividadeId: string;
  tipo: TipoCertificado;
}
export interface CertificadoPorCursoResponse {
  id: string;
  grupo: string;
  categoria: string;
  tituloAtividade: string;
  cargaHoraria: number;
  local: string;
  dataInicio: string;
  dataFim: string;
  status: string;
  tipo: string;
  alunoId: string;
  alunoNome: string;
  alunoEmail: string;
  alunoMatricula: string;
  periodoTurma: string;
  justificativaRejeicao?: string;
  cargaHorariaOriginal?: number;
  cargaHorariaCorrigida: boolean;
}

export const listarPeriodosLetivosValidos = async (): Promise<string[]> => {
  const response = await api.get<string[]>('/Certificado/periodos-validos');
  return response.data;
};

export const enviarCertificado = async (
  form: FormData
): Promise<{ certificadoId: string }> => {
  const response = await api.post('/Certificado', form, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
  return response.data;
};

export const listarCertificados = async (
  status?: StatusCertificado,
  alunoId?: string
): Promise<CertificadoResponse[]> => {
  const params = new URLSearchParams();
  if (status) params.append('status', status);
  if (alunoId) params.append('alunoId', alunoId);

  const response = await api.get<CertificadoResponse[]>(
    `/Certificado?${params}`
  );
  return response.data;
};

export const obterCertificadoPorId = async (
  id: string
): Promise<CertificadoDetalhadoResponse> => {
  const response = await api.get<CertificadoDetalhadoResponse>(
    `/Certificado/${id}`
  );
  return response.data;
};

export const aprovarCertificado = async (
  id: string,
  novaCargaHoraria?: number
): Promise<void> => {
  await api.patch(
    `/Certificado/${id}/aprovar`,
    novaCargaHoraria !== undefined ? { novaCargaHoraria } : {}
  );
};

export const reprovarCertificado = async (
  id: string,
  justificativa: string
): Promise<void> => {
  await api.patch(`/Certificado/${id}/reprovar`, { justificativa });
};

export const listarMeusCertificados = async (): Promise<
  CertificadoResponse[]
> => {
  const response = await api.get<CertificadoResponse[]>('/Certificado/me');
  return response.data;
};

export const listarCertificadosPorCurso = async (
  cursoId: string
): Promise<CertificadoPorCursoResponse[]> => {
  const response = await api.get<CertificadoPorCursoResponse[]>(
    `/Certificado/por-curso/${cursoId}`
  );
  return response.data;
};

export const baixarAnexoCertificado = async (id: string): Promise<Blob> => {
  const response = await api.get(`/Certificado/${id}/anexo`, {
    responseType: 'blob'
  });
  return response.data;
};

export interface UpdateCertificadoRequest {
  tituloAtividade?: string;
  instituicao?: string;
  local?: string;
  categoria?: string;
  grupo?: string;
  periodoLetivo?: string;
  cargaHoraria?: number;
  dataInicio?: string;
  dataFim?: string;
  totalPeriodos?: number;
  descricao?: string;
  anexo?: File;
  atividadeId?: string;
  tipo?: TipoCertificado;
}

// Atualizar certificado por ID (apenas PENDENTE)
export const atualizarCertificado = async (
  id: string,
  form: FormData
): Promise<void> => {
  await api.put(`/Certificado/${id}`, form, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
};

// Deletar certificado por ID (apenas ADMIN/COORDENADOR)
export const deletarCertificado = async (id: string): Promise<void> => {
  if (!id) {
    throw new Error('ID do certificado é obrigatório');
  }

  const cleanId = id.trim();

  try {
    await api.delete(`/Certificado/${cleanId}`);
  } catch (error) {
    throw error;
  }
};
