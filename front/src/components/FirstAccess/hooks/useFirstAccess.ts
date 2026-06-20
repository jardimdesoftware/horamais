import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';

import { useCriarAluno } from '@/hooks/useCriarAluno';
import { extractApiError } from '@/lib/apiError';
import { verificarTurmaExiste } from '@/services/classService';
import { zodResolver } from '@hookform/resolvers/zod';

import { firstAccessSchema, FirstAccessSchema } from '../schemas/schema';

export const useFirstAccess = () => {
  const router = useRouter();

  const [step, setStep] = useState(1);
  const [codigo, setCodigo] = useState('');
  const [turma, setTurma] = useState<{ codigo: string; nome: string } | null>(
    null
  );
  const [loading, setLoading] = useState(false);

  const { mutateAsync: criarAlunoAsync, isPending: isCriandoAluno } =
    useCriarAluno();

  const form = useForm<FirstAccessSchema>({
    resolver: zodResolver(firstAccessSchema),
    mode: 'onChange'
  });

  const handleValidarCodigo = async () => {
    try {
      setLoading(true);
      const turmaData = await verificarTurmaExiste(codigo.trim());

      if (!turmaData) {
        toast.error(
          'Código inválido. Solicite ao coordenador ou à secretaria.'
        );
        return;
      }

      const nomeTurma = `Turma de ${turmaData.cursoNome} ${turmaData.periodo}`;
      setTurma({ codigo: codigo.trim(), nome: nomeTurma });
      setStep(2);
    } catch (error) {
      toast.error(
        extractApiError(
          error,
          'Erro ao validar código. Tente novamente mais tarde.'
        )
      );
    } finally {
      setLoading(false);
    }
  };

  const handleFinalizarCadastro = async (data: FirstAccessSchema) => {
    if (!turma) return;

    try {
      await criarAlunoAsync({ ...data, turmaCodigo: turma.codigo });
      toast.success(
        'Cadastro realizado com sucesso! Você pode acessar o sistema agora.'
      );
      router.push('/');
    } catch (err) {
      toast.error(extractApiError(err, 'Erro ao cadastrar. Tente novamente.'));
    }
  };

  return {
    step,
    setStep,
    codigo,
    setCodigo,
    turma,
    form,
    loading: loading || isCriandoAluno,
    handleValidarCodigo,
    handleFinalizarCadastro
  };
};
