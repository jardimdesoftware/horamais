import React from 'react';
import { Controller } from 'react-hook-form';
import {
  FaClock,
  FaFileAlt,
  FaCalendarAlt,
  FaMapMarkerAlt,
  FaBookOpen,
  FaBuilding,
  FaAlignLeft,
  FaExclamationTriangle
} from 'react-icons/fa';

import { CustomFileInput } from '@/components/CustomFileInput';
import SelectBox from '@components/ui/SelectBox';

import { AtividadeResponse } from '@/services/activityService';

import { useHoursRegistrationForm } from './hooks/useHoursRegistrationForm';
import { ESPECIFICACAO_MAX_LENGTH } from './schemas/hoursRegistrationFormSchema';

interface HoursRegistrationFormProps {
  categoriasComplementares: AtividadeResponse[];
  categoriasExtensao: AtividadeResponse[];
  periodosLetivos: string[];
}

export default function HoursRegistrationForm({
  categoriasComplementares,
  categoriasExtensao,
  periodosLetivos
}: Readonly<HoursRegistrationFormProps>) {
  const {
    formMethods,
    control,
    handleSubmit,
    submitForm,
    handleFileSelect,
    handleTipoChange,
    anexoComprovante,
    isLoading,
    errors,
    tipoRegistro,
    maxHorasSemestral,
    campoHorasHabilitado
  } = useHoursRegistrationForm({
    categoriasComplementares,
    categoriasExtensao
  });

  const { register } = formMethods;
  const [termosExpandidos, setTermosExpandidos] = React.useState(false);

  const especificacaoLength =
    formMethods.watch('especificacaoAtividade')?.length ?? 0;

  const categoriasAtuais =
    tipoRegistro === 'horas-extensao'
      ? categoriasExtensao
      : categoriasComplementares;

  const inputClass =
    'w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500';
  const errorClass = 'text-red-500 text-xs mt-1';

  return (
    <div className="min-h-screen w-full flex items-center justify-center bg-gray-50 overflow-x-auto">
      <div className="w-full max-w-5xl mx-auto p-4 md:p-8 bg-white rounded-2xl shadow-md overflow-hidden">
        <div className="flex mb-6 rounded-lg border border-gray-200 overflow-hidden w-fit">
          <button
            type="button"
            onClick={() => handleTipoChange('horas-complementares')}
            className={`px-5 py-2 text-sm font-medium transition-colors ${
              tipoRegistro === 'horas-complementares'
                ? 'bg-blue-600 text-white'
                : 'bg-white text-gray-600 hover:bg-gray-50'
            }`}
          >
            Horas Complementares
          </button>
          <button
            type="button"
            onClick={() => handleTipoChange('horas-extensao')}
            className={`px-5 py-2 text-sm font-medium transition-colors ${
              tipoRegistro === 'horas-extensao'
                ? 'bg-blue-600 text-white'
                : 'bg-white text-gray-600 hover:bg-gray-50'
            }`}
          >
            Horas de Extensão
          </button>
        </div>

        <form
          onSubmit={handleSubmit(submitForm)}
          className="grid grid-cols-1 md:grid-cols-3 gap-6"
        >
          <div className="col-span-1">
            <label htmlFor="tituloAtividade" className="block mb-1 font-medium">
              <span className="flex items-center gap-2">
                <FaFileAlt className="text-blue-600" /> Título da Atividade
              </span>
            </label>
            <input
              id="tituloAtividade"
              type="text"
              {...register('tituloAtividade')}
              placeholder="Digite o título da atividade"
              className={inputClass}
            />
            {errors.tituloAtividade && (
              <p className={errorClass}>{errors.tituloAtividade.message}</p>
            )}
          </div>

          <div className="col-span-1">
            <label htmlFor="instituicao" className="block mb-1 font-medium">
              <span className="flex items-center gap-2">
                <FaBuilding className="text-blue-600" /> Instituição
              </span>
            </label>
            <input
              id="instituicao"
              type="text"
              {...register('instituicao')}
              placeholder="Digite o nome da instituição"
              className={inputClass}
            />
            {errors.instituicao && (
              <p className={errorClass}>{errors.instituicao.message}</p>
            )}
          </div>

          <div className="col-span-1">
            <label htmlFor="localRealizacao" className="block mb-1 font-medium">
              <span className="flex items-center gap-2">
                <FaMapMarkerAlt className="text-blue-600" /> Local de Realização
              </span>
            </label>
            <input
              id="localRealizacao"
              type="text"
              {...register('localRealizacao')}
              placeholder="Ex: Auditório principal"
              className={inputClass}
            />
            {errors.localRealizacao && (
              <p className={errorClass}>{errors.localRealizacao.message}</p>
            )}
          </div>

          <div className="col-span-1">
            <label htmlFor="categoria" className="block mb-1 font-medium">
              <span className="flex items-center gap-2">
                <FaFileAlt className="text-blue-600" /> Categoria
              </span>
            </label>
            <Controller
              control={control}
              name="categoria"
              render={({ field }) => (
                <SelectBox
                  value={field.value}
                  onChange={field.onChange}
                  options={categoriasAtuais.map((c) => ({
                    value: c.nome,
                    label: `${c.nome}`
                  }))}
                  {...(errors.categoria?.message
                    ? { error: errors.categoria.message }
                    : {})}
                />
              )}
            />
          </div>

          <div className="col-span-1">
            <label
              htmlFor="periodoLetivoFaculdade"
              className="block mb-1 font-medium"
            >
              <span className="flex items-center gap-2">
                <FaBookOpen className="text-blue-600" /> Período Letivo
              </span>
            </label>
            <select
              id="periodoLetivoFaculdade"
              {...register('periodoLetivoFaculdade')}
              className={inputClass}
            >
              <option value="">Selecione</option>
              {periodosLetivos.map((periodo) => (
                <option key={periodo} value={periodo}>
                  {periodo}
                </option>
              ))}
            </select>
            {errors.periodoLetivoFaculdade && (
              <p className={errorClass}>
                {errors.periodoLetivoFaculdade.message}
              </p>
            )}
          </div>

          <div className="col-span-1">
            <label htmlFor="cargaHoraria" className="block mb-1 font-medium">
              <span className="flex items-center gap-2">
                <FaClock className="text-blue-600" /> Carga Horária
              </span>
            </label>
            <Controller
              control={control}
              name="cargaHoraria"
              render={({ field }) => (
                <input
                  id="cargaHoraria"
                  type="number"
                  value={field.value}
                  disabled={!campoHorasHabilitado}
                  min={1}
                  max={maxHorasSemestral ?? undefined}
                  onChange={(e) => {
                    const val = Number(e.target.value);
                    field.onChange(
                      maxHorasSemestral !== null && val > maxHorasSemestral
                        ? maxHorasSemestral
                        : val
                    );
                  }}
                  className={`${inputClass} ${!campoHorasHabilitado ? 'bg-gray-100 cursor-not-allowed text-gray-400' : ''}`}
                />
              )}
            />
            {campoHorasHabilitado && maxHorasSemestral !== null && (
              <p className="text-xs text-gray-500 mt-1">
                Máximo permitido neste semestre:{' '}
                <span className="font-medium">{maxHorasSemestral}h</span>
              </p>
            )}
            {!campoHorasHabilitado && (
              <p className="text-xs text-gray-400 mt-1">
                Selecione a categoria e o período letivo primeiro.
              </p>
            )}
            {errors.cargaHoraria && (
              <p className={errorClass}>{errors.cargaHoraria.message}</p>
            )}
          </div>

          <div className="col-span-1">
            <label
              htmlFor="dataInicioAtividade"
              className="block mb-1 font-medium"
            >
              <span className="flex items-center gap-2">
                <FaCalendarAlt className="text-blue-600" /> Data de Início
              </span>
            </label>
            <input
              id="dataInicioAtividade"
              type="date"
              {...register('dataInicioAtividade')}
              className={inputClass}
            />
            {errors.dataInicioAtividade && (
              <p className={errorClass}>{errors.dataInicioAtividade.message}</p>
            )}
          </div>

          <div className="col-span-1">
            <label
              htmlFor="dataFimAtividade"
              className="block mb-1 font-medium"
            >
              <span className="flex items-center gap-2">
                <FaCalendarAlt className="text-blue-600" /> Data de Fim
              </span>
            </label>
            <input
              id="dataFimAtividade"
              type="date"
              {...register('dataFimAtividade')}
              className={inputClass}
            />
            {errors.dataFimAtividade && (
              <p className={errorClass}>{errors.dataFimAtividade.message}</p>
            )}
          </div>

          <div className="col-span-1">
            <label htmlFor="totalPeriodos" className="block mb-1 font-medium">
              <span className="flex items-center gap-2">
                <FaBookOpen className="text-blue-600" /> Total de Períodos
              </span>
            </label>
            <input
              id="totalPeriodos"
              type="number"
              {...register('totalPeriodos')}
              className={inputClass}
              min={1}
            />
            {errors.totalPeriodos && (
              <p className={errorClass}>{errors.totalPeriodos.message}</p>
            )}
          </div>

          <div className="col-span-1 md:col-span-3">
            <label
              htmlFor="especificacaoAtividade"
              className="block mb-1 font-medium"
            >
              <span className="flex items-center gap-2">
                <FaAlignLeft className="text-blue-600" /> Especificação da
                Atividade
              </span>
            </label>
            <textarea
              id="especificacaoAtividade"
              {...register('especificacaoAtividade')}
              maxLength={ESPECIFICACAO_MAX_LENGTH}
              className={`${inputClass} h-24`}
              rows={3}
              placeholder="Descreva brevemente a atividade realizada, local e período."
            />
            <div className="flex justify-between items-start gap-2 mt-1">
              <div className="flex-1">
                {errors.especificacaoAtividade && (
                  <p className={errorClass}>
                    {errors.especificacaoAtividade.message}
                  </p>
                )}
              </div>
              <span className="text-xs text-gray-500 whitespace-nowrap">
                {especificacaoLength}/{ESPECIFICACAO_MAX_LENGTH}
              </span>
            </div>
          </div>

          <div className="col-span-1 md:col-span-3">
            <label
              htmlFor="anexoComprovante"
              className="block mb-1 font-medium"
            >
              <span className="flex items-center gap-2">
                <FaFileAlt className="text-blue-600" /> Anexar Comprovante
              </span>
            </label>
            <Controller
              name="anexoComprovante"
              control={control}
              render={({ fieldState: { error } }) => (
                <>
                  <CustomFileInput
                    selectedFile={anexoComprovante}
                    onFileSelect={handleFileSelect}
                    disabled={isLoading}
                  />
                  {error && <p className={errorClass}>{error.message}</p>}
                </>
              )}
            />
            <p className="text-xs text-gray-500 mt-1">
              Tipos aceitos: PDF, JPG, PNG. Tamanho máx: 5MB.
            </p>
          </div>

          <div className="col-span-1 md:col-span-3">
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
              <div className="flex items-start gap-3">
                <input
                  id="aceitarTermos"
                  type="checkbox"
                  {...register('aceitarTermos')}
                  className="mt-1 w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 cursor-pointer"
                />
                <label
                  htmlFor="aceitarTermos"
                  className="text-sm text-gray-700 cursor-pointer flex-1"
                >
                  <div className="flex items-start gap-2">
                    <FaExclamationTriangle className="text-yellow-600 mt-0.5 flex-shrink-0" />
                    <div className="flex-1">
                      <span>
                        Declaro, sob as penas da lei, que todas as informações e
                        documentos apresentados são verdadeiros e condizentes{' '}
                        <button
                          type="button"
                          onClick={(e) => {
                            e.preventDefault();
                            setTermosExpandidos(!termosExpandidos);
                          }}
                          className="text-blue-600 hover:text-blue-800 underline font-medium"
                        >
                          {termosExpandidos ? 'Ler menos' : 'Leia mais'}
                        </button>{' '}
                        com a realidade dos fatos.
                      </span>
                      {termosExpandidos && (
                        <div className="mt-2 pl-6 text-gray-700 leading-relaxed animate-fadeIn">
                          <p className="mb-2">
                            Estou ciente de que a falsidade nas informações ou
                            documentos apresentados implicará em penalidades
                            cabíveis, conforme previsto no Código Penal
                            Brasileiro, especialmente nos artigos que tratam de
                            crimes contra a fé pública e falsidade documental,
                            podendo resultar na anulação do registro das horas
                            complementares ou de extensão, bem como em medidas
                            disciplinares pertinentes.
                          </p>
                        </div>
                      )}
                    </div>
                  </div>
                </label>
              </div>
              {errors.aceitarTermos && (
                <p className="text-red-500 text-xs mt-2 pl-6">
                  {errors.aceitarTermos.message}
                </p>
              )}
            </div>
          </div>

          <div className="col-span-1 md:col-span-3 flex justify-end pt-4">
            <button
              type="submit"
              disabled={isLoading || !formMethods.watch('aceitarTermos')}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Enviando...' : 'Enviar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
