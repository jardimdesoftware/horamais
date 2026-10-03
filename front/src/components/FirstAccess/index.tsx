'use client';

import Image from 'next/image';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import {
  faEnvelope,
  faUser,
  faIdBadge,
  faKey
} from '@fortawesome/free-solid-svg-icons';

import { useFirstAccess } from './hooks/useFirstAccess';

export const FirstAccess = () => {
  const {
    isGoogleFirstAccess,
    registrationTicket,
    restartGoogle,
    isPendingVerification,
    step,
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
  } = useFirstAccess();

  const {
    register,
    handleSubmit,
    formState: { errors, isValid }
  } = form;

  return (
    <div className="min-h-screen grid md:grid-cols-2 w-full">
      <div className="flex items-start md:items-center justify-center bg-white pt-4 pb-0">
        <Image
          src="/login.svg"
          alt="Primeiro Acesso"
          width={400}
          height={400}
          className="w-auto h-auto max-w-[300px] md:max-w-[400px]"
        />
      </div>

      <div className="flex flex-col justify-center items-center px-6 md:px-12 pb-4 md:pb-0">
        <div className="w-full max-w-md">
          <h1 className="text-3xl md:text-4xl font-bold text-center text-[#1351B4] mb-6">
            Primeiro Acesso
          </h1>

          {isGoogleFirstAccess && step < 3 && (
            <p className="text-sm text-gray-700 mb-4 text-center">
              Complete seu cadastro com o código da turma e sua matrícula.
              Depois, você entrará com sua conta Google, sem criar senha.
            </p>
          )}

          {isGoogleFirstAccess && !registrationTicket && step < 3 && (
            <div className="text-center space-y-4">
              <p className="text-sm text-gray-700">
                Sua autorização do Google expirou. Entre novamente para
                continuar o cadastro.
              </p>
              <Button type="button" onClick={restartGoogle} className="w-full">
                Entrar com Google
              </Button>
            </div>
          )}

          {!isGoogleFirstAccess && step < 3 && (
            <div className="text-center space-y-4">
              <p className="text-sm text-gray-700">
                Alunos fazem o primeiro acesso com a conta Google institucional.
              </p>
              <Button type="button" onClick={restartGoogle} className="w-full">
                Entrar com Google
              </Button>
            </div>
          )}

          {step === 1 && isGoogleFirstAccess && registrationTicket && (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                handleValidarCodigo();
              }}
            >
              <label className="block mb-1 text-sm">Código</label>
              <Input
                placeholder="Ex: ADS2B7"
                icon={faKey}
                value={codigo}
                onChange={(e) => setCodigo(e.target.value)}
              />
              <div className="flex items-center gap-2 bg-[#1351B4] text-white text-sm p-2 rounded-md mb-6 mt-0.5">
                <span>
                  Digite o código de 6 caracteres fornecido pelo seu coordenador
                </span>
              </div>

              <Button onClick={handleValidarCodigo} className="w-full">
                Continuar
              </Button>
            </form>
          )}

          {step === 2 && turma && isGoogleFirstAccess && registrationTicket && (
            <form onSubmit={handleSubmit(handleFinalizarCadastro)}>
              <p className="text-sm text-gray-700 mb-4 text-center">
                Entrando na turma: <strong>{turma.nome}</strong>
              </p>

              <div className="mb-3">
                <label className="block mb-1 text-sm">Nome:</label>
                <Input
                  placeholder="Nome completo"
                  icon={faUser}
                  {...register('nome')}
                />
                {errors.nome && (
                  <p className="text-xs text-red-500">{errors.nome.message}</p>
                )}
              </div>

              <div className="mb-3">
                <label className="block mb-1 text-sm">Email:</label>
                <Input
                  placeholder="Digite seu email institucional"
                  icon={faEnvelope}
                  readOnly={isGoogleFirstAccess}
                  {...register('email')}
                />
                {errors.email && (
                  <p className="text-xs text-red-500">{errors.email.message}</p>
                )}
              </div>

              <div className="mb-3">
                <label className="block mb-1 text-sm">Matrícula:</label>
                <Input
                  placeholder="Matrícula"
                  icon={faIdBadge}
                  {...register('matricula')}
                />
                {errors.matricula && (
                  <p className="text-xs text-red-500">
                    {errors.matricula.message}
                  </p>
                )}
              </div>

              <Button
                type="submit"
                disabled={loading || !isValid}
                className="w-full"
              >
                {loading ? 'Finalizando...' : 'Finalizar'}
              </Button>
            </form>
          )}

          {step === 3 && (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                handleConfirmarEmail();
              }}
            >
              <p className="text-sm text-gray-700 mb-4 text-center">
                {isPendingVerification
                  ? 'Seu cadastro já foi iniciado para '
                  : 'Enviamos um código de 6 dígitos para '}
                <strong>{emailCadastrado}</strong>.
                {isPendingVerification
                  ? ' Insira o código de verificação ou peça um novo abaixo.'
                  : ' Insira-o abaixo para ativar sua conta.'}
              </p>

              <label className="block mb-1 text-sm">
                Código de verificação
              </label>
              <Input
                placeholder="000000"
                icon={faKey}
                value={codigoVerificacao}
                onChange={(e) =>
                  setCodigoVerificacao(
                    e.target.value.replace(/\D/g, '').slice(0, 6)
                  )
                }
              />
              <div className="flex items-center gap-2 bg-[#1351B4] text-white text-sm p-2 rounded-md mb-6 mt-0.5">
                <span>
                  Não encontrou? Verifique também a caixa de spam. O código
                  expira em 24 horas.
                </span>
              </div>

              <Button
                type="submit"
                disabled={loading || codigoVerificacao.length !== 6}
                className="w-full"
              >
                {loading ? 'Confirmando...' : 'Confirmar e-mail'}
              </Button>

              <button
                type="button"
                onClick={handleReenviar}
                disabled={loading}
                className="w-full mt-3 text-sm text-[#1351B4] font-semibold hover:underline disabled:opacity-50"
              >
                Reenviar código
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};
