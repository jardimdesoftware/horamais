'use client';

import { useEffect, useMemo, useState } from 'react';
import { FaExclamationTriangle, FaUserClock } from 'react-icons/fa';
import { toast } from 'react-toastify';

import LoadingOverlay from '@/components/LoadingOverlay';
import { Badge } from '@/components/ui/badge';
import { BreadcrumbAuto } from '@/components/ui/breadcrumb';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';

import {
  listarAlunosEmRisco,
  type AlunoEmRiscoResponse
} from '@/services/studentService';

const FAIXAS = [
  { label: 'Crítico (< 25%)', value: 25 },
  { label: 'Em risco (< 50%)', value: 50 },
  { label: 'Atenção (< 75%)', value: 75 },
  { label: 'Todos', value: 100 }
];

const PAGE_SIZE = 20;

function corProgresso(pct: number) {
  if (pct < 25) return 'text-red-600';
  if (pct < 50) return 'text-orange-500';
  if (pct < 75) return 'text-yellow-600';
  return 'text-green-600';
}

function badgeRisco(pct: number) {
  if (pct < 25) return { label: 'Crítico', variant: 'destructive' as const };
  if (pct < 50) return { label: 'Em risco', variant: 'secondary' as const };
  return { label: 'Atenção', variant: 'outline' as const };
}

export default function AlunosEmRiscoPage() {
  // cache: todos os alunos até 100% — faixas filtram localmente
  const [todos, setTodos] = useState<AlunoEmRiscoResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [percentual, setPercentual] = useState(50);
  const [pagina, setPagina] = useState(1);

  useEffect(() => {
    const carregar = async () => {
      try {
        setLoading(true);
        const data = await listarAlunosEmRisco(100);
        setTodos(data);
      } catch {
        toast.error('Erro ao carregar alunos em risco.');
      } finally {
        setLoading(false);
      }
    };
    carregar();
  }, []);

  const alunosFiltrados = useMemo(
    () => todos.filter((a) => a.porcentagemConclusao <= percentual),
    [todos, percentual]
  );

  const totalPaginas = Math.max(
    1,
    Math.ceil(alunosFiltrados.length / PAGE_SIZE)
  );
  const paginaAtual = Math.min(pagina, totalPaginas);
  const alunosPagina = alunosFiltrados.slice(
    (paginaAtual - 1) * PAGE_SIZE,
    paginaAtual * PAGE_SIZE
  );

  const handleFaixa = (valor: number) => {
    setPercentual(valor);
    setPagina(1);
  };

  return (
    <div className="p-4 md:p-6 max-w-5xl mx-auto space-y-6">
      <LoadingOverlay show={loading} />

      <BreadcrumbAuto />

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl md:text-3xl font-semibold text-gray-800 flex items-center gap-2">
            <FaUserClock className="text-orange-500" />
            Alunos em Risco
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Ordenados por horas acumuladas por período (menor ritmo primeiro)
          </p>
        </div>

        <div className="flex flex-wrap gap-2">
          {FAIXAS.map((f) => (
            <Button
              key={f.value}
              size="sm"
              variant={percentual === f.value ? 'default' : 'outline'}
              onClick={() => handleFaixa(f.value)}
              className="cursor-pointer"
            >
              {f.label}
            </Button>
          ))}
        </div>
      </div>

      {!loading && alunosFiltrados.length === 0 && (
        <div className="text-center py-16 text-gray-400">
          <FaExclamationTriangle className="mx-auto w-10 h-10 mb-3" />
          <p className="text-lg font-medium">
            Nenhum aluno encontrado nessa faixa.
          </p>
        </div>
      )}

      <div className="space-y-3">
        {alunosPagina.map((aluno) => {
          const risco = badgeRisco(aluno.porcentagemConclusao);
          // Ritmo esperado: total de horas exigidas dividido pela duração do
          // curso em períodos (não pelos períodos já decorridos).
          const horasEsperadas =
            aluno.duracaoEmPeriodos > 0
              ? aluno.maximoHorasComplementar / aluno.duracaoEmPeriodos
              : 0;
          return (
            <div
              key={aluno.id}
              className="border rounded-lg p-4 bg-white space-y-3"
            >
              <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-2">
                <div>
                  <p className="font-semibold text-gray-900">{aluno.nome}</p>
                  <p className="text-sm text-gray-500">{aluno.matricula}</p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {aluno.cursoNome} · Turma {aluno.turmaPeriodo}
                  </p>
                </div>

                <div className="flex items-center gap-2 flex-wrap">
                  <Badge variant={risco.variant}>{risco.label}</Badge>
                  <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded-full font-medium">
                    atual: {aluno.horasPorPeriodo.toFixed(1)} h/período
                  </span>
                  <span className="text-xs bg-blue-50 text-blue-700 px-2 py-1 rounded-full font-medium">
                    esperado: {horasEsperadas.toFixed(1)} h/período
                  </span>
                  <span className="text-xs text-gray-500">
                    {aluno.periodosDecorridos} período
                    {aluno.periodosDecorridos !== 1 ? 's' : ''}
                  </span>
                </div>
              </div>

              <div className="space-y-1">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">
                    {aluno.totalHorasComplementar}h de{' '}
                    {aluno.maximoHorasComplementar}h complementares
                  </span>
                  <span
                    className={`font-semibold ${corProgresso(aluno.porcentagemConclusao)}`}
                  >
                    {aluno.porcentagemConclusao.toFixed(0)}%
                  </span>
                </div>
                <Progress value={aluno.porcentagemConclusao} className="h-2" />
              </div>
            </div>
          );
        })}
      </div>

      {totalPaginas > 1 && (
        <div className="flex items-center justify-center gap-2 pt-2">
          <Button
            size="sm"
            variant="outline"
            disabled={paginaAtual === 1}
            onClick={() => setPagina((p) => p - 1)}
            className="cursor-pointer"
          >
            Anterior
          </Button>
          <span className="text-sm text-gray-600">
            {paginaAtual} / {totalPaginas}
          </span>
          <Button
            size="sm"
            variant="outline"
            disabled={paginaAtual === totalPaginas}
            onClick={() => setPagina((p) => p + 1)}
            className="cursor-pointer"
          >
            Próxima
          </Button>
        </div>
      )}
    </div>
  );
}
