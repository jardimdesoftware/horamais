import { criarAluno, CreateAlunoRequest } from '@/services/studentService';
import { useMutation, useQueryClient } from '@tanstack/react-query';

/**
 * Cria um aluno e invalida a lista de alunos no cache. Assim, qualquer tela que
 * renderize a query `['resumo-horas']` (via `useResumoHoras`) reflete a inclusão
 * automaticamente, sem o usuário precisar recarregar a página.
 */
export function useCriarAluno() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dados: CreateAlunoRequest) => criarAluno(dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resumo-horas'] });
      queryClient.invalidateQueries({ queryKey: ['turma-alunos'] });
    }
  });
}
