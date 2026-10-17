import api from './api';

export type UserRole = 'ADMIN' | 'COORDENADOR' | 'ALUNO' | 'USUARIO';
export interface ManagedUser {
  id: string;
  nome: string;
  email: string;
  cargo: UserRole;
  ativo: boolean;
  emailVerificado: boolean;
  cargoAposRebaixamento: UserRole;
  cursoId: string | null;
  numeroPortaria: string | null;
  dou: string | null;
}
export interface UserPage {
  items: ManagedUser[];
  total: number;
  page: number;
  pageSize: number;
}
export const roleLabels: Record<UserRole, string> = {
  ADMIN: 'Administrador',
  COORDENADOR: 'Coordenador',
  ALUNO: 'Aluno',
  USUARIO: 'Usuário'
};

export async function listUsers(
  search: string,
  page: number,
  signal?: AbortSignal
) {
  const response = await api.get<UserPage>('/usuarios', {
    params: { search, page },
    ...(signal ? { signal } : {})
  });
  return response.data;
}

export async function updateUser(
  id: string,
  change: { ativo?: boolean; administrador?: boolean }
) {
  await api.patch(`/usuarios/${encodeURIComponent(id)}`, change);
}

export async function assignCoordinator(
  id: string,
  details: {
    cursoId: string;
    nome: string;
    numeroPortaria: string;
    dou: string;
  }
) {
  await api.put(`/usuarios/${encodeURIComponent(id)}/coordenador`, details);
}

export async function createCoordinator(details: {
  email: string;
  cursoId: string;
  nome: string;
  numeroPortaria: string;
  dou: string;
}) {
  await api.post('/usuarios/coordenador', details);
}
