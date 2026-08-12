import {
  FaBookOpen,
  FaCheckCircle,
  FaDownload,
  FaFilePdf,
  FaHandsHelping,
  FaTimesCircle,
  FaUser
} from 'react-icons/fa';

import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';

import {
  AlunoPorTurmaDetalhadoResponse,
  TurmaResponse
} from '@/services/classService';

interface StudentCardProps {
  student: AlunoPorTurmaDetalhadoResponse;
  turma: TurmaResponse;
  onToggleStatus: (studentId: string) => void;
  onDownload: (
    studentId: string,
    categoria: 'complementar' | 'extensao'
  ) => void;
  onGenerateSummary: (studentId: string) => void;
  isDownloading?: boolean;
  isGeneratingSummary?: boolean;
}

export const StudentCard = ({
  student,
  turma,
  onToggleStatus,
  onDownload,
  onGenerateSummary,
  isDownloading = false,
  isGeneratingSummary = false
}: StudentCardProps) => {
  const complementarConcluido =
    student.maximoHorasComplementar > 0 &&
    student.totalHorasComplementar >= student.maximoHorasComplementar;

  const extensaoConcluida =
    turma.possuiExtensao &&
    student.maximoHorasExtensao > 0 &&
    student.totalHorasExtensao >= student.maximoHorasExtensao;

  const porcentagemComplementar =
    student.maximoHorasComplementar > 0
      ? Math.min(
          (student.totalHorasComplementar / student.maximoHorasComplementar) *
            100,
          100
        )
      : 0;

  const porcentagemExtensao =
    student.maximoHorasExtensao > 0
      ? Math.min(
          (student.totalHorasExtensao / student.maximoHorasExtensao) * 100,
          100
        )
      : 0;

  return (
    <div className="border rounded-lg p-4 space-y-3">
      <div className="flex flex-col md:flex-row items-start md:items-center justify-between space-y-4 md:space-y-0">
        <div className="flex items-center space-x-3">
          <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
            <FaUser className="w-5 h-5 text-blue-600" />
          </div>
          <div>
            <p className="font-semibold text-gray-900">{student.nome}</p>
            <div className="flex items-center space-x-2 text-sm text-gray-600">
              <FaBookOpen className="w-4 h-4 text-blue-600" />
              <span>
                {student.totalHorasComplementar} /{' '}
                {student.maximoHorasComplementar} horas complementares
              </span>
            </div>
            {turma.possuiExtensao && (
              <div className="flex items-center space-x-2 text-sm text-gray-600">
                <FaHandsHelping className="w-4 h-4 text-green-600" />
                <span>
                  {student.totalHorasExtensao} / {student.maximoHorasExtensao}{' '}
                  horas extensão
                </span>
              </div>
            )}
          </div>
        </div>

        <div className="flex flex-col sm:flex-row items-start sm:items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => onGenerateSummary(student.id)}
            disabled={isGeneratingSummary}
            title="Gerar resumo em PDF para repasse à secretaria"
            className="text-gray-700 border-gray-300 hover:bg-gray-50 cursor-pointer"
          >
            <FaFilePdf className="w-3 h-3 mr-1" />
            Resumo
          </Button>
          {complementarConcluido && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => onDownload(student.id, 'complementar')}
              disabled={isDownloading}
              className="text-blue-700 border-blue-300 hover:bg-blue-50 cursor-pointer"
            >
              <FaDownload className="w-3 h-3 mr-1" />
              Complementar
            </Button>
          )}
          {extensaoConcluida && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => onDownload(student.id, 'extensao')}
              disabled={isDownloading}
              className="text-green-700 border-green-300 hover:bg-green-50 cursor-pointer"
            >
              <FaDownload className="w-3 h-3 mr-1" />
              Extensão
            </Button>
          )}
          <Badge
            variant={student.isAtivo ? 'default' : 'secondary'}
            className="flex items-center space-x-1"
          >
            {student.isAtivo ? (
              <FaCheckCircle className="w-3 h-3" />
            ) : (
              <FaTimesCircle className="w-3 h-3" />
            )}
            <span>{student.isAtivo ? 'Ativo' : 'Inativo'}</span>
          </Badge>
          <Button
            variant={student.isAtivo ? 'destructive' : 'default'}
            size="sm"
            onClick={() => onToggleStatus(student.id)}
            className="cursor-pointer"
          >
            {student.isAtivo ? 'Desativar' : 'Ativar'}
          </Button>
        </div>
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between text-sm">
          <span className="flex items-center gap-1.5">
            <FaBookOpen className="w-3.5 h-3.5 text-blue-600" />
            Horas complementares
          </span>
          <span>{porcentagemComplementar.toFixed(0)}%</span>
        </div>
        <Progress
          value={porcentagemComplementar}
          className="h-2 [&>[data-slot=progress-indicator]]:bg-blue-600"
        />

        {turma.possuiExtensao && (
          <>
            <div className="flex items-center justify-between text-sm">
              <span className="flex items-center gap-1.5">
                <FaHandsHelping className="w-3.5 h-3.5 text-green-600" />
                Horas de extensão
              </span>
              <span>{porcentagemExtensao.toFixed(0)}%</span>
            </div>
            <Progress
              value={porcentagemExtensao}
              className="h-2 [&>[data-slot=progress-indicator]]:bg-green-600"
            />
          </>
        )}
      </div>
    </div>
  );
};
