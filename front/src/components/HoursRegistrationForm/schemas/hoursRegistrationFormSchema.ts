import { z } from 'zod';

// Mensagem de erro padrão para campos obrigatórios
const requiredError = (fieldName: string) => `${fieldName} é obrigatório(a).`;

// Limite de caracteres para a especificação da atividade
export const ESPECIFICACAO_MAX_LENGTH = 500;

// Schema para o arquivo (comprovante)
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ACCEPTED_FILE_TYPES = [
  'application/pdf',
  'image/jpeg',
  'image/jpg',
  'image/png'
];

export const hoursRegistrationFormSchema = z
  .object({
    tituloAtividade: z
      .string(requiredError('Título da Atividade'))
      .min(3, 'O título deve ter pelo menos 3 caracteres.'),
    instituicao: z
      .string(requiredError('Instituição'))
      .min(2, 'O nome da instituição deve ter pelo menos 2 caracteres.'),
    localRealizacao: z
      .string(requiredError('Local de Realização/Participação'))
      .min(3, 'O local deve ter pelo menos 3 caracteres.'),
    especificacaoAtividade: z
      .string(requiredError('Especificação das Atividades'))
      .min(10, 'A descrição deve ter pelo menos 10 caracteres.')
      .max(
        ESPECIFICACAO_MAX_LENGTH,
        `A descrição deve ter no máximo ${ESPECIFICACAO_MAX_LENGTH} caracteres.`
      ),
    categoria: z
      .string(requiredError('Categoria'))
      .min(1, 'Selecione uma categoria.'),
    cargaHoraria: z
      .string(requiredError('Carga Horária')) // Manter como string para o input
      .refine((value) => !isNaN(parseFloat(value)) && parseFloat(value) > 0, {
        message: 'Carga horária deve ser um número positivo.'
      })
      .transform((value) => parseFloat(value)), // Transforma para número após validação
    dataInicioAtividade: z
      .string(requiredError('Data de Início'))
      .refine((val) => !isNaN(Date.parse(val)), {
        message: 'Data de início inválida.'
      }),
    dataFimAtividade: z
      .string(requiredError('Data de Fim'))
      .refine((val) => !isNaN(Date.parse(val)), {
        message: 'Data de fim inválida.'
      }),
    totalPeriodos: z
      .string(requiredError('Total de Períodos'))
      .refine((value) => !isNaN(parseFloat(value)) && parseFloat(value) >= 0, {
        message: 'Total de períodos deve ser um número não negativo.'
      })
      .transform((value) => parseFloat(value)), // Transforma para número após validação
    periodoLetivoFaculdade: z
      .string(requiredError('Período Letivo'))
      .regex(
        /^\d{4}\.[1-2]$/,
        'Formato inválido. Use AAAA.1 ou AAAA.2 (ex: 2023.1).'
      ),
    anexoComprovante: z
      .custom<File | null>((file) => file instanceof File, {
        message: 'Comprovante é obrigatório.'
      })
      .refine((file) => file && file.size <= MAX_FILE_SIZE, {
        message: `O arquivo deve ter no máximo ${MAX_FILE_SIZE / 1024 / 1024}MB.`
      })
      .refine((file) => file && ACCEPTED_FILE_TYPES.includes(file.type), {
        message:
          'Tipo de arquivo inválido. Apenas PDF, JPG, JPEG ou PNG são aceitos.'
      }),
    aceitarTermos: z
      .boolean({
        message: 'Você deve aceitar os termos para continuar.'
      })
      .refine((value) => value === true, {
        message: 'Você deve aceitar os termos de veracidade das informações.'
      })
  })
  .refine(
    (data) => {
      // Validação para garantir que data de fim não seja anterior à data de início
      if (data.dataInicioAtividade && data.dataFimAtividade) {
        return (
          new Date(data.dataFimAtividade) >= new Date(data.dataInicioAtividade)
        );
      }
      return true;
    },
    {
      message: 'A data de fim não pode ser anterior à data de início.',
      path: ['dataFimAtividade'] // Campo onde o erro será associado
    }
  );

// Tipo inferido a partir do schema para usar no formulário
export type HoursRegistrationFormSchema = z.infer<
  typeof hoursRegistrationFormSchema
>;
