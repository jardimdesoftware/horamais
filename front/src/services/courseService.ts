import api from './api';

export interface CampusResponse {
  id: string;
  nome: string;
  cidade: string;
}

export interface CursoResponse {
  id: string;
  nome: string;
}

export interface CursoDetalhadoResponse {
  id: string;
  nome: string;
  maximoHorasComplementar: number;
  duracaoEmPeriodos: number;
  campusId: string;
  nomeCampus: string;
}

export interface CreateCursoRequest {
  nomeCurso: string;
  maximoHorasComplementar: number;
  duracaoEmPeriodos: number;
  campusId: string;
}

export interface CursoResumoResponse {
  id: string;
  nome: string;
  quantidadeTurmas: number;
  quantidadeAlunos: number;
  campusId: string;
  nomeCampus: string;
}
// Criar novo curso
export const criarCurso = async (
  dados: CreateCursoRequest
): Promise<CursoResponse> => {
  const response = await api.post<CursoResponse>('/Curso', dados);
  return response.data;
};

// Listar todos os campi
export const listarCampuses = async (): Promise<CampusResponse[]> => {
  const response = await api.get<CampusResponse[]>('/Campus');
  return response.data;
};

// Listar todos os cursos (opcionalmente filtrado por campus)
export const listarCursos = async (
  campusId?: string
): Promise<CursoResponse[]> => {
  const params = campusId ? { campusId } : {};
  const response = await api.get<CursoResponse[]>('/Curso', { params });
  return response.data;
};

// Obter curso por ID (inclui limites de horas)
export const obterCursoPorId = async (
  id: string
): Promise<CursoDetalhadoResponse> => {
  const response = await api.get<CursoDetalhadoResponse>(`/Curso/${id}`);
  return response.data;
};

export const obterResumoCursos = async (): Promise<CursoResumoResponse[]> => {
  try {
    const response = await api.get('/Curso/resumo');

    const data = response?.data;

    if (Array.isArray(data?.data)) {
      return data.data;
    }

    if (Array.isArray(data)) {
      return data;
    }

    return [];
  } catch {
    return [];
  }
};

export interface UpdateCursoRequest {
  nomeCurso: string;
  maximoHorasComplementar: number;
  duracaoEmPeriodos: number;
  campusId: string;
}

// Atualizar curso por ID (atualiza também os limites de horas)
export const atualizarCurso = async (
  id: string,
  dados: UpdateCursoRequest
): Promise<void> => {
  await api.put(`/Curso/${id}`, dados);
};

// Deletar curso por ID
export const deletarCurso = async (cursoId: string): Promise<void> => {
  if (!cursoId) {
    throw new Error('ID do curso é obrigatório');
  }

  const cleanId = cursoId.trim().replace(/^\/+|\/+$/g, '');
  const guidRegex =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

  if (!guidRegex.test(cleanId)) {
    throw new Error(
      'ID do curso inválido. Formato esperado: GUID (ex: 00000000-0000-0000-0000-000000000000).'
    );
  }

  try {
    await api.request({ method: 'DELETE', url: `/Curso/${cleanId}` });
  } catch (error: unknown) {
    const err = error as { response?: { status?: number } };
    if (err?.response?.status === 405) {
      throw new Error(
        'Método não permitido. Verifique se o endpoint DELETE está habilitado no backend.'
      );
    }
    throw error;
  }
};
