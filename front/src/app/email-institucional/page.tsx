'use client';

import { signIn } from 'next-auth/react';
import Image from 'next/image';
import Link from 'next/link';
import { useState } from 'react';
import { FcGoogle } from 'react-icons/fc';

import { Button } from '@/components/ui/button';

export default function EmailInstitucionalPage() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(false);

  const escolherConta = async () => {
    setLoading(true);
    setError(false);
    try {
      await signIn(
        'google',
        { callbackUrl: '/' },
        { prompt: 'select_account' }
      );
    } catch {
      setError(true);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen grid md:grid-cols-2 w-full">
      <div className="flex items-center justify-center px-6 pt-8 md:py-8">
        <Image
          src="/login.svg"
          alt=""
          width={400}
          height={400}
          className="w-auto h-auto max-w-[240px] md:max-w-[400px]"
        />
      </div>
      <div className="flex items-center justify-center px-6 py-10 md:px-12">
        <div className="w-full max-w-md text-center">
          <p className="text-sm font-semibold text-primary mb-3">Hora Mais</p>
          <h1 className="text-3xl font-semibold text-primary mb-5">
            Vamos usar seu e-mail institucional?
          </h1>
          <p className="text-gray-700 leading-relaxed">
            A conta Google escolhida não é um e-mail de discente do IFPE. Para
            começar seu cadastro, entre com sua conta institucional.
          </p>
          <div className="my-6 rounded-xl bg-blue-50 px-4 py-5 text-blue-900">
            <p className="text-sm mb-1">Seu e-mail deve terminar com</p>
            <p className="font-semibold break-words">@discente.ifpe.edu.br</p>
          </div>
          <Button
            type="button"
            onClick={escolherConta}
            disabled={loading}
            className="w-full gap-2"
          >
            <FcGoogle
              aria-hidden="true"
              className="bg-white rounded-full p-0.5 size-6"
            />
            {loading ? 'Abrindo o Google...' : 'Escolher outra conta Google'}
          </Button>
          {error && (
            <p role="alert" className="mt-3 text-sm text-red-600">
              Não conseguimos abrir o Google agora. Tente novamente em
              instantes.
            </p>
          )}
          <Link
            href="/"
            className="inline-block mt-5 text-sm font-semibold text-primary underline underline-offset-4"
          >
            Voltar para o login
          </Link>
          <p className="mt-8 text-sm text-gray-600 leading-relaxed">
            Ainda não sabe qual é seu e-mail institucional? Procure a secretaria
            ou a coordenação do seu curso para receber orientação.
          </p>
        </div>
      </div>
    </main>
  );
}
