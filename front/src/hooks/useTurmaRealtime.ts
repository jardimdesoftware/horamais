import { useEffect } from 'react';

import { turmaAlunosKey } from '@/hooks/useTurmaAlunos';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';

// O hub é mapeado na raiz do backend (/hubs/turma), fora do prefixo /api dos
// controllers. Derivamos a URL a partir da mesma env da API.
const apiBase = process.env.NEXT_PUBLIC_API_URL || '/api';
const HUB_URL = `${apiBase.replace(/\/api\/?$/, '')}/hubs/turma`;

/**
 * Mantém uma conexão SignalR com o grupo da turma aberta. Quando um aluno entra
 * na turma (em qualquer sessão), o servidor empurra "AlunosAlterados" e nós
 * invalidamos a lista — que então é refeita via REST autenticado. Resultado: o
 * aluno aparece ao vivo, sem recarregar a página.
 *
 * Dois identificadores entram em jogo, de propósito:
 * - `grupoTurmaId`: o GUID canônico da turma. É o nome do grupo SignalR e PRECISA
 *   casar com o que o backend usa para publicar (turma.Id). A URL pode trazer o
 *   código da turma em vez do GUID, então não dá para usar o id da rota aqui.
 * - `queryKeyId`: o identificador usado na queryKey da lista de alunos (o mesmo
 *   que a página passou para `useAlunosPorTurma`), para a invalidação bater.
 *
 * Roda só no client (useEffect), nunca no SSR.
 */
export function useTurmaRealtime(
  grupoTurmaId: string | undefined,
  queryKeyId: string | undefined
) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!grupoTurmaId || !queryKeyId) return;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    let cancelled = false;
    const entrarNaTurma = () => connection.invoke('EntrarTurma', grupoTurmaId);

    connection.on('AlunosAlterados', () => {
      queryClient.invalidateQueries({ queryKey: turmaAlunosKey(queryKeyId) });
    });

    // O servidor perde os grupos quando a conexão cai; reentra ao reconectar.
    connection.onreconnected(() => {
      if (!cancelled) entrarNaTurma();
    });

    connection
      .start()
      .then(() => {
        if (!cancelled) void entrarNaTurma();
      })
      .catch(() => {
        // Silencioso: o tempo real é um aprimoramento. Sem ele, a tela continua
        // funcionando via REST (refetch ao focar a aba).
      });

    return () => {
      cancelled = true;
      connection.stop();
    };
  }, [grupoTurmaId, queryKeyId, queryClient]);
}
