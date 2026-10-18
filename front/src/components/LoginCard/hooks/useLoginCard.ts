'use client';

import { signIn } from 'next-auth/react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useEffect } from 'react';
import { toast } from 'react-toastify';

export function useLoginCard() {
  const router = useRouter();
  const searchParams = useSearchParams();

  useEffect(() => {
    const error = searchParams.get('error');
    if (!error) return;
    const messages: Record<string, string> = {
      GoogleDomainNotAllowed: 'Use sua conta Google institucional do IFPE.',
      GoogleAccountInactive:
        'Sua conta está inativa. Entre em contato com a administração.',
      GoogleTokenInvalid:
        'Não foi possível validar o acesso com Google. Tente novamente.'
    };
    toast.error(
      messages[error] ?? 'Não foi possível entrar com Google. Tente novamente.'
    );
    router.replace('/');
  }, [searchParams, router]);

  return { submitGoogle: () => signIn('google', { callbackUrl: '/' }) };
}
