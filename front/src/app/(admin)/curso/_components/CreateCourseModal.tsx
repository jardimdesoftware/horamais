import React, { useEffect, useState } from 'react';
import { FaGraduationCap, FaTimes } from 'react-icons/fa';
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
  CreateCursoRequest,
  criarCurso,
  listarCampuses
} from '@/services/courseService';

interface CreateCourseModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateCourseModal = ({
  isOpen,
  onClose,
  onSuccess
}: CreateCourseModalProps) => {
  const [newCourseName, setNewCourseName] = useState('');
  const [complementaryHours, setComplementaryHours] = useState('');
  const [durationPeriods, setDurationPeriods] = useState('8');
  const [selectedCampusId, setSelectedCampusId] = useState('');
  const [campuses, setCampuses] = useState<CampusResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [confirmCreate, setConfirmCreate] = useState(false);

  useEffect(() => {
    if (!isOpen) return;
    listarCampuses()
      .then(setCampuses)
      .catch(() => toast.error('Não foi possível carregar os campi.'));
  }, [isOpen]);

  const handleAddCourse = (e: React.FormEvent) => {
    e.preventDefault();

    if (!newCourseName.trim()) {
      toast.error('O nome do curso é obrigatório.');
      return;
    }

    if (!selectedCampusId) {
      toast.error('Selecione um campus.');
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

    setConfirmCreate(true);
  };

  const executeCreateCourse = async () => {
    try {
      setLoading(true);

      const payload: CreateCursoRequest = {
        nomeCurso: newCourseName,
        maximoHorasComplementar: Number(complementaryHours),
        duracaoEmPeriodos: Number(durationPeriods),
        campusId: selectedCampusId
      };

      await criarCurso(payload);

      toast.success(`O curso "${newCourseName}" foi criado com sucesso.`);

      onSuccess();
      onClose();
      setNewCourseName('');
      setComplementaryHours('');
      setDurationPeriods('8');
      setSelectedCampusId('');
    } catch (error) {
      toast.error(extractApiError(error, 'Não foi possível criar o curso.'));
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  const campusOptions = campuses.map((c) => ({
    value: c.id,
    label: c.nome
  }));

  return (
    <>
      <AlertDialog open={confirmCreate} onOpenChange={setConfirmCreate}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Tem certeza?</AlertDialogTitle>
            <AlertDialogDescription>
              Deseja criar o curso &quot;{newCourseName}&quot;?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={executeCreateCourse}>
              Sim, criar
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
                  <FaGraduationCap className="w-8 h-8 text-blue-600" />
                </div>
                <CardTitle>Adicionar Novo Curso</CardTitle>
                <CardDescription>
                  Preencha todos os campos para criar um novo curso
                </CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleAddCourse} className="space-y-6">
                  <div className="space-y-2">
                    <label htmlFor="courseName" className="block font-medium">
                      Nome do curso
                    </label>
                    <input
                      id="courseName"
                      type="text"
                      placeholder="Nome do curso"
                      value={newCourseName}
                      onChange={(e) => setNewCourseName(e.target.value)}
                      required
                      className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-600"
                    />
                  </div>

                  <div className="space-y-2">
                    <label
                      htmlFor="complementaryHours"
                      className="block font-medium"
                    >
                      Total de Horas Complementares
                    </label>
                    <input
                      id="complementaryHours"
                      type="number"
                      placeholder="Horas complementares"
                      value={complementaryHours}
                      onChange={(e) => setComplementaryHours(e.target.value)}
                      required
                      className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-600"
                    />
                  </div>

                  <div className="space-y-2">
                    <label
                      htmlFor="durationPeriods"
                      className="block font-medium"
                    >
                      Duração do curso (períodos)
                    </label>
                    <input
                      id="durationPeriods"
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
                      !newCourseName.trim() ||
                      !complementaryHours.trim() ||
                      !durationPeriods.trim() ||
                      !selectedCampusId
                    }
                    className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-50 cursor-pointer"
                  >
                    {loading ? 'Criando...' : 'Criar Curso'}
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
