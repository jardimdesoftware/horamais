'use client';

import Image from 'next/image';
import { FcGoogle } from 'react-icons/fc';

import { Button } from '@/components/ui/button';

import { useRedirectIfAuthenticated } from '@/hooks/useRedirectIfAuthenticated';

import { useLoginCard } from './hooks/useLoginCard';

export const LoginCard = () => {
  useRedirectIfAuthenticated();
  const { submitGoogle } = useLoginCard();

  return (
    <div className="min-h-screen grid md:grid-cols-2 w-full">
      <div className="flex items-start md:items-center justify-center pt-4 pb-0">
        <Image
          src="/login.svg"
          alt="Acesso ao Hora Mais"
          width={400}
          height={400}
          className="w-auto h-auto max-w-[300px] md:max-w-[400px]"
        />
      </div>
      <div className="flex flex-col justify-center items-center px-6 md:px-12 pb-4 md:pb-0">
        <div className="w-full max-w-md text-center">
          <h1 className="text-3xl lg:text-4xl font-medium text-primary mb-4">
            Hora Mais
          </h1>
          <p className="mb-8 text-sm text-gray-600">
            Acesse com sua conta Google institucional do IFPE.
          </p>
          <Button type="button" className="w-full" onClick={submitGoogle}>
            <FcGoogle className="h-5 w-5" />
            Entrar com Google
          </Button>
        </div>
      </div>
    </div>
  );
};
