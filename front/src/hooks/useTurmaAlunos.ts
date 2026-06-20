import {
  listarAlunosPorTurma,
  obterTurmaPorId,
  AlunoPorTurmaDetalhadoResponse,
  TurmaResponse
} from '@/services/classService';
import { useQuery } from '@tanstack/react-query';

/**
 * Chave da lista de alunos de uma turma. Centralizada para que tanto as
 * mutations (toggle de status, futura criação) quanto o futuro evento de
 * tempo real (SignalR) invalidem exatamente a mesma query.
 */
export const turmaAlunosKey = (turmaId: string) => ['turma-alunos', turmaId];

export function useTurma(turmaId: string | undefined) {
  return useQuery<TurmaResponse>({
    queryKey: ['turma', turmaId],
    queryFn: () => obterTurmaPorId(turmaId!),
    enabled: !!turmaId
  });
}

export function useAlunosPorTurma(turmaId: string | undefined) {
  return useQuery<AlunoPorTurmaDetalhadoResponse[]>({
    queryKey: turmaAlunosKey(turmaId ?? ''),
    queryFn: () => listarAlunosPorTurma(turmaId!),
    enabled: !!turmaId,
    // Faz o aluno recém-cadastrado aparecer quando o coordenador volta o foco
    // para a aba, sem recarregar a página manualmente.
    refetchOnWindowFocus: true
  });
}
