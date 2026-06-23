import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';

import { useCriarAluno } from '@/hooks/useCriarAluno';
import { extractApiError } from '@/lib/apiError';
import { confirmEmail, resendVerification } from '@/services/authRecovery';
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

  // E-mail do cadastro pendente e código de verificação (etapa 3)
  const [emailCadastrado, setEmailCadastrado] = useState('');
  const [codigoVerificacao, setCodigoVerificacao] = useState('');

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
      setEmailCadastrado(data.email);
      toast.success(
        'Cadastro iniciado! Enviamos um código de verificação para o seu e-mail.'
      );
      setStep(3);
    } catch (err) {
      toast.error(extractApiError(err, 'Erro ao cadastrar. Tente novamente.'));
    }
  };

  const handleConfirmarEmail = async () => {
    if (!emailCadastrado || codigoVerificacao.length !== 6) return;

    try {
      setLoading(true);
      await confirmEmail({
        email: emailCadastrado,
        code: codigoVerificacao
      });
      toast.success('E-mail confirmado! Você já pode acessar o sistema.');
      router.push('/');
    } catch (err) {
      toast.error(
        extractApiError(err, 'Código inválido ou expirado. Tente novamente.')
      );
    } finally {
      setLoading(false);
    }
  };

  const handleReenviar = async () => {
    if (!emailCadastrado) return;

    try {
      setLoading(true);
      await resendVerification({ email: emailCadastrado });
      toast.success('Enviamos um novo código. Verifique sua caixa de entrada.');
    } catch (err) {
      toast.error(
        extractApiError(err, 'Erro ao reenviar o código. Tente novamente.')
      );
    } finally {
      setLoading(false);
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
    emailCadastrado,
    codigoVerificacao,
    setCodigoVerificacao,
    handleValidarCodigo,
    handleFinalizarCadastro,
    handleConfirmarEmail,
    handleReenviar
  };
};
