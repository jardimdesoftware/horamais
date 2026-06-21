'use client';

import { useSession } from 'next-auth/react';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { FaBell } from 'react-icons/fa';

import { useCertificadoNotificacoes } from '@/hooks/useCertificadoNotificacoes';

const VALIDACAO_PATH = '/coordenacao/certificados';

/**
 * Notificação global de certificados pendentes para o coordenador. Fica no canto
 * superior direito e aparece em qualquer tela enquanto houver certificados a
 * validar. Ao clicar, leva à tela de validação.
 *
 * Combina baseline via REST (aparece mesmo se o certificado chegou enquanto o
 * coordenador estava em outra parte) com tempo real via SignalR (mesmo padrão do
 * tempo real da turma): o backend empurra "NovoCertificado" para o grupo do curso
 * quando um aluno envia um certificado.
 */
export function CertificadoNotificacao() {
  const { data: session } = useSession();
  const cursoId = session?.user?.cursoId;
  const pathname = usePathname();
  const router = useRouter();

  const { pendentes, refetch } = useCertificadoNotificacoes(cursoId);

  // Reatualiza ao trocar de tela (ex.: depois de validar e sair da validação),
  // já que o layout — e este componente — não remontam entre telas do coordenador.
  useEffect(() => {
    void refetch();
  }, [pathname, refetch]);

  if (!cursoId || pendentes === 0 || pathname === VALIDACAO_PATH) return null;

  const handleClick = () => router.push(VALIDACAO_PATH);

  const label =
    pendentes === 1
      ? '1 certificado para validar'
      : `${pendentes} certificados para validar`;

  return (
    <button
      type="button"
      onClick={handleClick}
      aria-label={`${label}. Clique para validar.`}
      className="fixed top-20 right-4 z-50 flex items-center gap-3 rounded-lg bg-blue-600 px-4 py-3 text-white shadow-lg transition-colors hover:bg-blue-700 cursor-pointer"
    >
      <span className="relative shrink-0">
        <FaBell className="h-5 w-5" />
        <span className="absolute -top-2 -right-2 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-xs font-bold">
          {pendentes > 99 ? '99+' : pendentes}
        </span>
      </span>
      <span className="text-left">
        <span className="block text-sm font-medium">{label}</span>
        <span className="block text-xs text-blue-100">Clique para validar</span>
      </span>
    </button>
  );
}
