'use client';

import { FaSave, FaTimes, FaPlus } from 'react-icons/fa';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

import { useActivityForm } from './hooks/useActivityForm';
import { ACTIVITY_MAX_LENGTHS, ActivityFormSchema } from './schemas/schema';

interface Props {
  tipo: 'EXTENSAO' | 'COMPLEMENTAR';
  onSubmit: (data: ActivityFormSchema) => void;
  onCancel: () => void;
  isVisible: boolean;
}

export function ActivityForm({ tipo, onSubmit, onCancel, isVisible }: Props) {
  const {
    isSubmitting,
    register,
    handleSubmit,
    errors,
    watch,
    setValue,
    handleFormSubmit
  } = useActivityForm({ onSubmit });

  const possuiCurricularizacao = watch('possuiCurricularizacaoExtensao');

  if (!isVisible) return null;

  return (
    <Card className="shadow-elegant animate-scale-in">
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <FaPlus className="h-4 w-4 text-primary" />
            Nova Atividade
          </CardTitle>
          <Button
            variant="ghost"
            size="sm"
            onClick={onCancel}
            className="cursor-pointer"
          >
            <FaTimes className="h-4 w-4" />
          </Button>
        </div>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <Label htmlFor="nome">Nome da Atividade *</Label>
              <Input
                id="nome"
                {...register('nome')}
                maxLength={ACTIVITY_MAX_LENGTHS.nome}
                placeholder="Digite o nome da atividade"
              />
              {errors.nome && (
                <p className="text-sm text-destructive">
                  {errors.nome.message}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="grupo">Grupo *</Label>
              <Input
                id="grupo"
                {...register('grupo')}
                maxLength={ACTIVITY_MAX_LENGTHS.grupo}
                placeholder="Ex: I, II, III"
              />
              {errors.grupo && (
                <p className="text-sm text-destructive">
                  {errors.grupo.message}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="categoria">Categoria *</Label>
              <Input
                id="categoria"
                {...register('categoria')}
                maxLength={ACTIVITY_MAX_LENGTHS.categoria}
                placeholder="Ex: Categoria 1"
              />
              {errors.categoria && (
                <p className="text-sm text-destructive">
                  {errors.categoria.message}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="categoriaKey">Área *</Label>
              <Input
                id="categoriaKey"
                {...register('categoriaKey')}
                maxLength={ACTIVITY_MAX_LENGTHS.categoriaKey}
                placeholder="Ensino, Pesquisa, Extensão"
              />
              {errors.categoriaKey && (
                <p className="text-sm text-destructive">
                  {errors.categoriaKey.message}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="cargaMaximaSemestral">
                Carga Máxima Semestral *
              </Label>
              <Input
                id="cargaMaximaSemestral"
                type="number"
                {...register('cargaMaximaSemestral', { valueAsNumber: true })}
                placeholder="Ex: 60"
              />
              {errors.cargaMaximaSemestral && (
                <p className="text-sm text-destructive">
                  {errors.cargaMaximaSemestral.message}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="cargaMaximaCurso">Carga Máxima do Curso *</Label>
              <Input
                id="cargaMaximaCurso"
                type="number"
                {...register('cargaMaximaCurso', { valueAsNumber: true })}
                placeholder="Ex: 120"
              />
              {errors.cargaMaximaCurso && (
                <p className="text-sm text-destructive">
                  {errors.cargaMaximaCurso.message}
                </p>
              )}
            </div>
          </div>

          {tipo === 'EXTENSAO' && (
            <div className="border-t pt-4 space-y-4">
              <div>
                <Label>
                  Esta atividade possui curricularização de extensão? *
                </Label>
                <div className="flex gap-4 mt-2">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="curricularizacao"
                      value="sim"
                      checked={possuiCurricularizacao === true}
                      onChange={() =>
                        setValue('possuiCurricularizacaoExtensao', true, {
                          shouldValidate: true
                        })
                      }
                      className="accent-primary"
                    />
                    Sim
                  </label>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="curricularizacao"
                      value="nao"
                      checked={possuiCurricularizacao === false}
                      onChange={() => {
                        setValue('possuiCurricularizacaoExtensao', false, {
                          shouldValidate: true
                        });
                        setValue('horasCurricularizacaoExtensao', undefined);
                      }}
                      className="accent-primary"
                    />
                    Não
                  </label>
                </div>
              </div>

              {possuiCurricularizacao === true && (
                <div>
                  <Label htmlFor="horasCurricularizacaoExtensao">
                    Horas de curricularização *
                  </Label>
                  <Input
                    id="horasCurricularizacaoExtensao"
                    type="number"
                    min={1}
                    {...register('horasCurricularizacaoExtensao', {
                      valueAsNumber: true
                    })}
                    placeholder="Ex: 40"
                  />
                  {errors.horasCurricularizacaoExtensao && (
                    <p className="text-sm text-destructive">
                      {errors.horasCurricularizacaoExtensao.message}
                    </p>
                  )}
                </div>
              )}
            </div>
          )}

          <div className="flex gap-3 pt-4 border-t">
            <Button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 cursor-pointer"
            >
              <FaSave className="h-4 w-4 mr-2" />
              {isSubmitting ? 'Salvando...' : 'Salvar Atividade'}
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={onCancel}
              className="cursor-pointer"
            >
              Cancelar
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
