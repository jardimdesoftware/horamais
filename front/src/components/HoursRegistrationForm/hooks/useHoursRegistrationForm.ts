import { useSession } from 'next-auth/react';
import { useSearchParams } from 'next/navigation';
import { useRouter } from 'next/navigation';
import { useState, useMemo, useCallback, useEffect } from 'react';
import { SubmitHandler, useForm, useWatch } from 'react-hook-form';
import { toast } from 'react-toastify';

import { HoursRegistrationFormSchema } from '@components/HoursRegistrationForm/schemas/hoursRegistrationFormSchema';

import { AtividadeResponse } from '@/services/activityService';
import { enviarCertificado } from '@/services/certificateService';

export interface UseHoursRegistrationFormProps {
  categoriasComplementares: AtividadeResponse[];
  categoriasExtensao: AtividadeResponse[];
}

export function useHoursRegistrationForm({
  categoriasComplementares,
  categoriasExtensao
}: UseHoursRegistrationFormProps) {
  const searchParams = useSearchParams();
  const { data: session } = useSession();
  const router = useRouter();

  const [tipoRegistro, setTipoRegistro] = useState(
    searchParams.get('tipo') ?? 'horas-complementares'
  );

  const [isUploading, setIsUploading] = useState(false);

  const form = useForm<HoursRegistrationFormSchema>({
    defaultValues: {
      tituloAtividade: '',
      instituicao: '',
      localRealizacao: '',
      categoria: '',
      periodoLetivoFaculdade: '',
      cargaHoraria: 0,
      dataInicioAtividade: '',
      dataFimAtividade: '',
      totalPeriodos: 1,
      especificacaoAtividade: '',
      anexoComprovante: null,
      aceitarTermos: false
    },
    mode: 'onChange'
  });

  const {
    control,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting }
  } = form;

  const anexoComprovante = useWatch({ control, name: 'anexoComprovante' });
  const categoriaWatched = useWatch({ control, name: 'categoria' });
  const periodoWatched = useWatch({ control, name: 'periodoLetivoFaculdade' });

  const categoriasAtuais = useMemo(() => {
    return tipoRegistro === 'horas-extensao'
      ? categoriasExtensao
      : categoriasComplementares;
  }, [tipoRegistro, categoriasComplementares, categoriasExtensao]);

  const atividadeSelecionada = useMemo(
    () => categoriasAtuais.find((c) => c.nome === categoriaWatched),
    [categoriasAtuais, categoriaWatched]
  );

  const maxHorasSemestral = atividadeSelecionada?.cargaMaximaSemestral ?? null;
  const campoHorasHabilitado = !!categoriaWatched && !!periodoWatched;

  useEffect(() => {
    setValue('cargaHoraria', 0, { shouldValidate: false });
  }, [categoriaWatched, setValue]);

  const handleTipoChange = useCallback(
    (novoTipo: string) => {
      setValue('categoria', '');
      setValue('cargaHoraria', 0, { shouldValidate: false });
      setTipoRegistro(novoTipo);
    },
    [setValue]
  );

  const handleFileSelect = (file: File | null) => {
    setValue('anexoComprovante', file, {
      shouldValidate: true,
      shouldDirty: true
    });
  };

  const handleFileRemove = () => {
    setValue('anexoComprovante', null, {
      shouldValidate: true,
      shouldDirty: true
    });
  };

  const submitForm: SubmitHandler<HoursRegistrationFormSchema> = async (
    data
  ) => {
    try {
      if (!session?.user?.entidadeId || !session?.user?.cursoId) {
        toast.error('Usuário não autenticado ou curso não identificado.');
        return;
      }

      const atividadeSelecionada = categoriasAtuais.find(
        (c) => c.nome === data.categoria
      );
      if (!atividadeSelecionada) {
        toast.error('Categoria não encontrada ou inválida.');
        return;
      }

      const dataInicioUTC = new Date(
        data.dataInicioAtividade + 'T00:00:00Z'
      ).toISOString();
      const dataFimUTC = new Date(
        data.dataFimAtividade + 'T00:00:00Z'
      ).toISOString();

      const formData = new FormData();
      formData.append('TituloAtividade', data.tituloAtividade);
      formData.append('Instituicao', data.instituicao);
      formData.append('Local', data.localRealizacao);
      formData.append('Categoria', atividadeSelecionada.categoria);
      formData.append('Grupo', atividadeSelecionada.grupo);
      formData.append('PeriodoLetivo', data.periodoLetivoFaculdade);
      formData.append('CargaHoraria', data.cargaHoraria.toString());
      formData.append('DataInicio', dataInicioUTC);
      formData.append('DataFim', dataFimUTC);
      formData.append('TotalPeriodos', data.totalPeriodos.toString());
      if (data.especificacaoAtividade) {
        formData.append('Descricao', data.especificacaoAtividade);
      }
      formData.append('AlunoId', session.user.entidadeId);
      formData.append('AtividadeId', atividadeSelecionada.id);
      formData.append('Tipo', tipoRegistro === 'horas-extensao' ? '0' : '1');
      if (data.anexoComprovante) {
        formData.append('Anexo', data.anexoComprovante);
      }

      setIsUploading(true);
      await enviarCertificado(formData);

      toast.success('Certificado enviado com sucesso!');
      form.reset();
      router.push('/aluno/certificado');
    } catch (error: unknown) {
      const mensagem =
        (error as { response?: { data?: { erro?: string } } })?.response?.data
          ?.erro ?? 'Não foi possível enviar o certificado.';
      toast.error(mensagem);
    } finally {
      setIsUploading(false);
    }
  };

  return {
    formMethods: form,
    control,
    handleSubmit,
    submitForm,
    handleFileSelect,
    handleFileRemove,
    handleTipoChange,
    anexoComprovante,
    isLoading: isSubmitting || isUploading,
    errors,
    tipoRegistro,
    maxHorasSemestral,
    campoHorasHabilitado
  };
}
