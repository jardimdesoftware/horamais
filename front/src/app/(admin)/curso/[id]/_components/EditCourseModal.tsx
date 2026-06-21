import React, { useEffect, useState } from 'react';
import { FaEdit, FaTimes } from 'react-icons/fa';
import { toast } from 'react-toastify';

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from '@/components/ui/alert-dialog';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import SelectBox from '@/components/ui/SelectBox';

import { extractApiError } from '@/lib/apiError';
import {
  CampusResponse,
  CursoDetalhadoResponse,
  UpdateCursoRequest,
  atualizarCurso,
  listarCampuses
} from '@/services/courseService';

interface EditCourseModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  curso: CursoDetalhadoResponse;
}

export const EditCourseModal = ({
  isOpen,
  onClose,
  onSuccess,
  curso
}: EditCourseModalProps) => {
  const [nomeCurso, setNomeCurso] = useState('');
  const [complementaryHours, setComplementaryHours] = useState('');
  const [durationPeriods, setDurationPeriods] = useState('8');
  const [selectedCampusId, setSelectedCampusId] = useState('');
  const [campuses, setCampuses] = useState<CampusResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [confirmEdit, setConfirmEdit] = useState(false);

  useEffect(() => {
    if (!isOpen) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setNomeCurso(curso.nome);
    setComplementaryHours(String(curso.maximoHorasComplementar));
    setDurationPeriods(String(curso.duracaoEmPeriodos));
    setSelectedCampusId(curso.campusId);
    listarCampuses()
      .then(setCampuses)
      .catch(() => toast.error('Não foi possível carregar os campi.'));
  }, [isOpen, curso]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!nomeCurso.trim()) {
      toast.error('O nome do curso é obrigatório.');
      return;
    }

    if (Number(complementaryHours) <= 0) {
      toast.error('O total de horas complementares deve ser maior que zero.');
      return;
    }

    if (Number(durationPeriods) <= 0) {
      toast.error('A duração em períodos deve ser maior que zero.');
      return;
    }

    if (!selectedCampusId) {
      toast.error('Selecione um campus.');
      return;
    }

    setConfirmEdit(true);
  };

  const executeUpdate = async () => {
    try {
      setLoading(true);

      const payload: UpdateCursoRequest = {
        nomeCurso: nomeCurso.trim(),
        maximoHorasComplementar: Number(complementaryHours),
        duracaoEmPeriodos: Number(durationPeriods),
        campusId: selectedCampusId
      };

      await atualizarCurso(curso.id, payload);

      toast.success('Curso atualizado com sucesso.');
      onSuccess();
      onClose();
    } catch (error) {
      toast.error(
        extractApiError(error, 'Não foi possível atualizar o curso.')
      );
    } finally {
      setLoading(false);
      setConfirmEdit(false);
    }
  };

  if (!isOpen) return null;

  const campusOptions = campuses.map((c) => ({
    value: c.id,
    label: c.nome
  }));

  return (
    <>
      <AlertDialog open={confirmEdit} onOpenChange={setConfirmEdit}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Confirmar alterações?</AlertDialogTitle>
            <AlertDialogDescription>
              Deseja salvar as alterações no curso &quot;{nomeCurso}&quot;? Os
              limites de horas serão atualizados para todos os alunos vinculados
              a este curso.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={executeUpdate} disabled={loading}>
              {loading ? 'Salvando...' : 'Sim, salvar'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <div className="fixed inset-0 bg-black/20 flex items-center justify-center z-50">
        <div className="bg-white rounded-lg overflow-auto max-h-full w-full max-w-2xl p-7 relative">
          <button
            className="absolute top-0 mt-1 mb-1 right-4 text-gray-500 hover:text-gray-700 cursor-pointer"
            onClick={onClose}
          >
            <FaTimes className="w-6 h-6" />
          </button>

          <div className="max-w-2xl mx-auto space-y-8">
            <Card>
              <CardHeader className="text-center">
                <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FaEdit className="w-8 h-8 text-blue-600" />
                </div>
                <CardTitle>Editar Curso</CardTitle>
                <CardDescription>
                  Atualize os dados e limites de horas do curso
                </CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleSubmit} className="space-y-6">
                  <div className="space-y-2">
                    <label
                      htmlFor="editCourseName"
                      className="block font-medium"
                    >
                      Nome do curso
                    </label>
                    <input
                      id="editCourseName"
                      type="text"
                      placeholder="Nome do curso"
                      value={nomeCurso}
                      onChange={(e) => setNomeCurso(e.target.value)}
                      required
                      className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-600"
                    />
                  </div>

                  <div className="space-y-2">
                    <label
                      htmlFor="editComplementaryHours"
                      className="block font-medium"
                    >
                      Total de Horas Complementares
                    </label>
                    <input
                      id="editComplementaryHours"
                      type="number"
                      min={1}
                      placeholder="Horas complementares"
                      value={complementaryHours}
                      onChange={(e) => setComplementaryHours(e.target.value)}
                      required
                      className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-600"
                    />
                  </div>

                  <div className="space-y-2">
                    <label
                      htmlFor="editDurationPeriods"
                      className="block font-medium"
                    >
                      Duração do curso (períodos)
                    </label>
                    <input
                      id="editDurationPeriods"
                      type="number"
                      min={1}
                      max={20}
                      placeholder="Ex: 8"
                      value={durationPeriods}
                      onChange={(e) => setDurationPeriods(e.target.value)}
                      required
                      className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-600"
                    />
                    <p className="text-xs text-gray-500">
                      Usado para calcular o ritmo esperado de horas (máximo ÷
                      duração).
                    </p>
                  </div>

                  <div className="space-y-2">
                    <label className="block font-medium">Campus</label>
                    <SelectBox
                      value={selectedCampusId}
                      onChange={setSelectedCampusId}
                      placeholder="Selecione um campus"
                      options={campusOptions}
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={
                      loading ||
                      !nomeCurso.trim() ||
                      !complementaryHours.trim() ||
                      !durationPeriods.trim() ||
                      !selectedCampusId
                    }
                    className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-50 cursor-pointer"
                  >
                    Salvar alterações
                  </button>
                </form>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </>
  );
};
