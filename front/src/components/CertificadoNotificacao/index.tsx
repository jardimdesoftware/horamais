'use client';

import { useSession } from 'next-auth/react';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { FaBell } from 'react-icons/fa';

import { useCertificadoNotificacoes } from '@/hooks/useCertificadoNotificacoes';

const VALIDACAO_PATH = '/coordenacao/certificados';

/**
 * Notificação global de chegada de certificados para o coordenador. Fica no
 * canto superior direito e aparece em qualquer tela enquanto houver novos
 * certificados a validar. Ao clicar, leva à tela de validação e zera o aviso.
 *
 * É alimentada por SignalR (mesmo padrão do tempo real da turma): o backend
 * empurra "NovoCertificado" para o grupo do curso quando um aluno envia um
 * certificado.
 */
export function CertificadoNotificacao() {
  const { data: session } = useSession();
  const cursoId = session?.user?.cursoId;
  const pathname = usePathname();
  const router = useRouter();

  const { novos, reset } = useCertificadoNotificacoes(cursoId);

  // Ao entrar na própria tela de validação, o aviso perde o sentido: zera.
  useEffect(() => {
    if (pathname === VALIDACAO_PATH) reset();
  }, [pathname, reset]);

  if (!cursoId || novos === 0 || pathname === VALIDACAO_PATH) return null;

  const handleClick = () => {
    reset();
    router.push(VALIDACAO_PATH);
  };

  const label =
    novos === 1
      ? 'Novo certificado recebido'
      : `${novos} novos certificados recebidos`;

  return (
    <button
      type="button"
      onClick={handleClick}
      aria-label={`${label}. Clique para validar.`}
      className="fixed top-20 right-4 z-50 flex items-center gap-3 rounded-lg bg-blue-600 px-4 py-3 text-white shadow-lg transition-colors hover:bg-blue-700 cursor-pointer"
    >
      <span className="relative flex-shrink-0">
        <FaBell className="h-5 w-5" />
        <span className="absolute -top-2 -right-2 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-xs font-bold">
          {novos > 99 ? '99+' : novos}
        </span>
      </span>
      <span className="text-left">
        <span className="block text-sm font-medium">{label}</span>
        <span className="block text-xs text-blue-100">Clique para validar</span>
      </span>
    </button>
  );
}
