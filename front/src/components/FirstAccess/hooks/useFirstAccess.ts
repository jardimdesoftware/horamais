import { signIn } from 'next-auth/react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useEffect, useState, useSyncExternalStore } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';

import { extractApiError } from '@/lib/apiError';
import { confirmEmail, resendVerification } from '@/services/authRecovery';
import { verificarTurmaExiste } from '@/services/classService';
import { criarAlunoGoogle } from '@/services/studentService';
import { zodResolver } from '@hookform/resolvers/zod';

import { firstAccessSchema, FirstAccessSchema } from '../schemas/schema';

const subscribeToHash = (onChange: () => void) => {
  window.addEventListener('hashchange', onChange);
  return () => window.removeEventListener('hashchange', onChange);
};

const getRegistrationTicket = () =>
  new URLSearchParams(window.location.hash.slice(1)).get('ticket') ?? '';

export const useFirstAccess = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const isGoogleFirstAccess = searchParams.get('google') === '1';
  const isPendingVerification = searchParams.get('verify') === '1';

  const [step, setStep] = useState(isPendingVerification ? 3 : 1);
  const [codigo, setCodigo] = useState('');
  const [turma, setTurma] = useState<{ codigo: string; nome: string } | null>(
    null
  );
  const [loading, setLoading] = useState(false);
  const registrationTicket = useSyncExternalStore(
    subscribeToHash,
    getRegistrationTicket,
    () => ''
  );

  // E-mail do cadastro pendente e código de verificação (etapa 3)
  const [emailCadastrado, setEmailCadastrado] = useState(
    isPendingVerification ? (searchParams.get('email') ?? '') : ''
  );
  const [codigoVerificacao, setCodigoVerificacao] = useState('');

  const form = useForm<FirstAccessSchema>({
    resolver: zodResolver(firstAccessSchema),
    mode: 'onChange',
    defaultValues: {}
  });

  useEffect(() => {
    if (!isGoogleFirstAccess) return;
    // A URL só preenche a tela; o backend identifica o e-mail pelo ticket assinado.
    form.setValue('email', searchParams.get('email') ?? '', {
      shouldValidate: true
    });
    form.setValue('nome', searchParams.get('nome') ?? '', {
      shouldValidate: true
    });
  }, [form, isGoogleFirstAccess, searchParams]);

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
      if (isGoogleFirstAccess) {
        if (!registrationTicket) {
          toast.error(
            'Cadastro com Google expirado. Entre com Google novamente.'
          );
          return;
        }
        setLoading(true);
        await criarAlunoGoogle({
          registrationTicket,
          nome: data.nome,
          matricula: data.matricula,
          turmaCodigo: turma.codigo
        });
        toast.success('Cadastro concluído! Entrando com Google...');
        await signIn('google', { callbackUrl: '/' });
        return;
      }

      await signIn('google', { callbackUrl: '/' });
    } catch (err) {
      const response = (err as { response?: { status?: number } })?.response;
      const message = extractApiError(
        err,
        'Erro ao cadastrar. Tente novamente.'
      );
      if (isGoogleFirstAccess) {
        toast.error(message);
        return;
      }
      if (
        response?.status === 502 ||
        (response?.status === 422 && message.includes('already taken'))
      ) {
        setEmailCadastrado(data.email);
        setStep(3);
        toast.error(
          response.status === 502
            ? 'Cadastro criado, mas o código não foi enviado. Use “Reenviar código”.'
            : 'Este cadastro já foi iniciado. Use o código enviado ou peça outro.'
        );
        return;
      }
      toast.error(message);
    } finally {
      if (isGoogleFirstAccess) setLoading(false);
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
    isGoogleFirstAccess,
    registrationTicket,
    restartGoogle: () => signIn('google', { callbackUrl: '/' }),
    isPendingVerification,
    step,
    setStep,
    codigo,
    setCodigo,
    turma,
    form,
    loading,
    emailCadastrado,
    codigoVerificacao,
    setCodigoVerificacao,
    handleValidarCodigo,
    handleFinalizarCadastro,
    handleConfirmarEmail,
    handleReenviar
  };
};
