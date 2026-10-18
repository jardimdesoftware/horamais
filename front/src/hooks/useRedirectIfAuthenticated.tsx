'use client';

import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { Atom } from 'react-loading-indicators';

import { COLORS } from '@/config/colors';

export const useRedirectIfAuthenticated = () => {
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === 'authenticated') {
      const role = session?.user.role;

      switch (role) {
        case 'admin':
          router.replace('/curso');
          break;
        case 'aluno':
          router.replace('/aluno');
          break;
        case 'coordenador':
          router.replace('/coordenacao');
          break;
        case 'usuario':
          router.replace('/conta');
          break;
        default:
          router.replace('/');
      }
    }
  }, [session, status, router]);

  if (status === 'loading') {
    return (
      <div className="flex justify-center items-center h-screen">
        <Atom
          color={COLORS.success}
          size="medium"
          text="Verificando sessão..."
          textColor="#333"
        />
      </div>
    );
  }

  return null;
};
