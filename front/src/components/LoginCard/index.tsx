'use client';

import Image from 'next/image';
import { FaCircleNotch } from 'react-icons/fa';
import { FcGoogle } from 'react-icons/fc';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import { faEnvelope, faLock } from '@fortawesome/free-solid-svg-icons';

import { useRedirectIfAuthenticated } from '../../hooks/useRedirectIfAuthenticated';
import { useLoginCard } from './hooks/useLoginCard';

export const LoginCard = () => {
  useRedirectIfAuthenticated();
  const {
    form: { register, handleSubmit, formState, watch },
    submitForm,
    submitGoogle
  } = useLoginCard();

  const { errors } = formState;
  const emailValue = watch('email');
  const isInstitutionalEmail =
    !!emailValue &&
    (emailValue.endsWith('@ifpe.edu.br') ||
      emailValue.endsWith('.ifpe.edu.br'));

  return (
    <div className="min-h-screen grid md:grid-cols-2 w-full">
      <div className="flex items-start md:items-center justify-center pt-4 pb-0">
        <Image
          src="/login.svg"
          alt="Login"
          width={400}
          height={400}
          className="w-auto h-auto max-w-[300px] md:max-w-[400px]"
        />
      </div>

      <div className="flex flex-col justify-center items-center px-6 md:px-12 pb-4 md:pb-0">
        <form onSubmit={handleSubmit(submitForm)} className="w-full max-w-md">
          <h1 className="text-3xl lg:text-4xl font-medium text-center text-primary mb-8">
            Hora Mais
          </h1>

          <div className="mb-4">
            <label htmlFor="email" className="block mb-1 text-sm">
              Email:
            </label>
            <Input
              id="email"
              type="email"
              placeholder="Digite seu email institucional."
              icon={faEnvelope}
              {...register('email')}
            />
            {errors.email && (
              <p className="text-xs text-red-500 mt-1">
                {errors.email.message}
              </p>
            )}
            {emailValue && !isInstitutionalEmail && (
              <p className="text-xs text-red-500 mt-1 font-medium">
                Use seu email institucional IFPE (ifpe.edu.br).
              </p>
            )}
          </div>

          <div className="mb-1">
            <label htmlFor="password" className="block mb-1 text-sm">
              Senha:
            </label>
            <Input
              id="password"
              isPassword
              icon={faLock}
              placeholder="Digite sua senha."
              {...register('password')}
            />

            {errors.password && (
              <p className="text-xs text-red-500 mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="text-right mb-4">
            <a
              href="esqueciSenha"
              className="text-sm text-foreground hover:underline cursor-pointer"
            >
              Esqueceu a senha?
            </a>
          </div>

          <div className="flex items-center mb-6">
            <input type="checkbox" id="keepConnected" className="mr-2" />
            <label htmlFor="keepConnected" className="text-sm">
              Manter-me conectado
            </label>
          </div>

          <div className="w-full">
            <Button
              type="submit"
              className="w-full"
              disabled={formState.isSubmitting}
            >
              {formState.isSubmitting && (
                <FaCircleNotch className="animate-spin mr-2" />
              )}
              {formState.isSubmitting ? 'Entrando...' : 'Entrar'}
            </Button>
          </div>

          <div className="flex items-center my-6">
            <hr className="flex-grow border-t-2 border-gray-300" />
            <span className="px-3 text-sm text-gray-500">ou</span>
            <hr className="flex-grow border-t-2 border-gray-300" />
          </div>

          <div className="w-full mb-6">
            <Button
              type="button"
              variant="outline"
              className="w-full"
              onClick={submitGoogle}
            >
              <FcGoogle className="h-5 w-5" />
              Entrar com Google
            </Button>
          </div>

          <div className="text-center">
            <a
              href="primeiroAcesso"
              className="text-primary font-medium hover:underline"
            >
              Primeiro Acesso?
            </a>
          </div>
        </form>
      </div>
    </div>
  );
};
