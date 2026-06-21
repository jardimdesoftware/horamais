import { useCallback, useEffect, useState } from 'react';

import {
  listarCertificadosPorCurso,
  StatusCertificado
} from '@/services/certificateService';
import { HubConnectionBuilder } from '@microsoft/signalr';

// O hub é mapeado na raiz do backend (/hubs/certificado), fora do prefixo /api
// dos controllers. Derivamos a URL a partir da mesma env da API.
const apiBase = process.env.NEXT_PUBLIC_API_URL || '/api';
const HUB_URL = `${apiBase.replace(/\/api\/?$/, '')}/hubs/certificado`;

/**
 * Mantém a contagem de certificados pendentes do curso do coordenador, usada
 * pela notificação global. A contagem é buscada via REST ao montar (para que o
 * aviso apareça mesmo se o coordenador não estava conectado quando o certificado
 * chegou) e atualizada em tempo real: o backend empurra "NovoCertificado" para o
 * grupo do curso (SignalR) e nós refazemos o fetch autenticado.
 *
 * O consumidor chama `refetch` para o baseline (ao montar e ao navegar); este
 * hook cuida da assinatura em tempo real e refaz o fetch a cada evento. Roda só
 * no client; sem a conexão, o baseline via REST ainda funciona.
 */
export function useCertificadoNotificacoes(cursoId: string | undefined) {
  const [pendentes, setPendentes] = useState(0);

  const refetch = useCallback(async () => {
    if (!cursoId) return;
    try {
      const lista = await listarCertificadosPorCurso(cursoId);
      setPendentes(
        lista.filter((c) => c.status === StatusCertificado.PENDENTE).length
      );
    } catch {
      // Silencioso: a notificação é um aprimoramento; falha de rede não quebra a tela.
    }
  }, [cursoId]);

  useEffect(() => {
    if (!cursoId) return;

    // O baseline (contagem ao montar e a cada navegação) é disparado pelo
    // consumidor via `refetch`; aqui cuidamos apenas da assinatura em tempo real.
    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    let cancelled = false;
    const entrarNoCurso = () => connection.invoke('EntrarCurso', cursoId);

    connection.on('NovoCertificado', () => {
      void refetch();
    });

    // O servidor perde os grupos quando a conexão cai; reentra ao reconectar.
    connection.onreconnected(() => {
      if (!cancelled) {
        entrarNoCurso();
        void refetch();
      }
    });

    connection
      .start()
      .then(() => {
        if (!cancelled) void entrarNoCurso();
      })
      .catch(() => {
        // Silencioso: sem tempo real, a validação continua acessível via REST.
      });

    return () => {
      cancelled = true;
      connection.stop();
    };
  }, [cursoId, refetch]);

  return { pendentes, refetch };
}
