import { useCallback, useEffect, useState } from 'react';

import { HubConnectionBuilder } from '@microsoft/signalr';

// O hub é mapeado na raiz do backend (/hubs/certificado), fora do prefixo /api
// dos controllers. Derivamos a URL a partir da mesma env da API.
const apiBase = process.env.NEXT_PUBLIC_API_URL || '/api';
const HUB_URL = `${apiBase.replace(/\/api\/?$/, '')}/hubs/certificado`;

/**
 * Escuta, via SignalR, a chegada de novos certificados no curso do coordenador.
 * Cada evento "NovoCertificado" incrementa um contador, que o componente de
 * notificação usa para avisar o coordenador em qualquer tela. `reset` zera a
 * contagem (ex.: ao abrir a tela de validação).
 *
 * Roda só no client (useEffect), nunca no SSR. Sem a conexão, a aplicação segue
 * funcionando normalmente — o tempo real é um aprimoramento.
 */
export function useCertificadoNotificacoes(cursoId: string | undefined) {
  const [novos, setNovos] = useState(0);

  useEffect(() => {
    if (!cursoId) return;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    let cancelled = false;
    const entrarNoCurso = () => connection.invoke('EntrarCurso', cursoId);

    connection.on('NovoCertificado', () => {
      setNovos((n) => n + 1);
    });

    // O servidor perde os grupos quando a conexão cai; reentra ao reconectar.
    connection.onreconnected(() => {
      if (!cancelled) entrarNoCurso();
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
  }, [cursoId]);

  const reset = useCallback(() => setNovos(0), []);

  return { novos, reset };
}
