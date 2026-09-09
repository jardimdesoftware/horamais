'use client';

import { signIn } from 'next-auth/react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useEffect } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { toast } from 'react-toastify';

import { zodResolver } from '@hookform/resolvers/zod';

import { loginSchema, type LoginSchemaType } from '../schemas/schema';

export function useLoginCard() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const form = useForm<LoginSchemaType>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: ''
    }
  });

  // Exibe apenas mensagens conhecidas, sem renderizar texto recebido pela URL.
  useEffect(() => {
    const error = searchParams.get('error');
    if (error) {
      const messages: Record<string, string> = {
        GoogleDomainNotAllowed:
          'Para criar sua conta com Google, use o e-mail @discente.ifpe.edu.br.',
        GoogleAccountNotFound:
          'Nenhuma conta encontrada para este e-mail. Cadastre-se no HoraMais com o mesmo e-mail da conta Google antes de entrar.',
        GoogleEmailNotConfirmed:
          'Confirme seu cadastro com o código enviado por e-mail antes de entrar com o Google.',
        GoogleAccountInactive:
          'Sua conta está inativa. Entre em contato com a coordenação.',
        GoogleTokenInvalid:
          'Não foi possível validar o login com o Google. Tente novamente.'
      };
      toast.error(
        messages[error] ??
          'Não foi possível entrar com o Google. Tente novamente em instantes.'
      );
      router.replace('/');
    }
  }, [searchParams, router]);

  const submitForm: SubmitHandler<LoginSchemaType> = async (data) => {
    const res = await signIn('credentials', {
      email: data.email,
      password: data.password,
      redirect: false
    });

    if (res?.ok) {
      const session = await fetch('/api/auth/session').then((res) =>
        res.json()
      );
      const role = session?.user?.role;

      toast.success('Login efetuado com sucesso!');

      switch (role) {
        case 'admin':
          router.push('/curso');
          break;
        case 'aluno':
          router.push('/aluno');
          break;
        case 'coordenador':
          router.push('/coordenacao');
          break;
        default:
          router.push('/');
      }
    } else {
      toast.error('Credenciais inválidas. Verifique seu e-mail e senha.');
    }
  };

  const submitGoogle = () => {
    signIn('google', { callbackUrl: '/' });
  };

  return {
    form,
    submitForm,
    submitGoogle
  };
}
