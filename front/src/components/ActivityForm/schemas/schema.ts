import * as z from 'zod';

// Limites de caracteres alinhados à validação do backend
export const ACTIVITY_MAX_LENGTHS = {
  nome: 200,
  grupo: 100,
  categoria: 100,
  categoriaKey: 50
} as const;

export const activitySchema = z
  .object({
    nome: z
      .string()
      .min(1, 'Nome é obrigatório')
      .min(3, 'Nome deve ter pelo menos 3 caracteres')
      .max(
        ACTIVITY_MAX_LENGTHS.nome,
        `Nome deve ter no máximo ${ACTIVITY_MAX_LENGTHS.nome} caracteres`
      ),
    grupo: z
      .string()
      .min(1, 'Grupo é obrigatório')
      .max(
        ACTIVITY_MAX_LENGTHS.grupo,
        `Grupo deve ter no máximo ${ACTIVITY_MAX_LENGTHS.grupo} caracteres`
      ),
    categoria: z
      .string()
      .min(1, 'Categoria é obrigatória')
      .max(
        ACTIVITY_MAX_LENGTHS.categoria,
        `Categoria deve ter no máximo ${ACTIVITY_MAX_LENGTHS.categoria} caracteres`
      ),
    categoriaKey: z
      .string()
      .min(1, 'Área é obrigatória')
      .max(
        ACTIVITY_MAX_LENGTHS.categoriaKey,
        `Área deve ter no máximo ${ACTIVITY_MAX_LENGTHS.categoriaKey} caracteres`
      ),
    cargaMaximaSemestral: z
      .number({ message: 'Carga semestral deve ser um número' })
      .positive('Deve ser maior que zero')
      .max(200, 'Máximo de 200 horas'),
    cargaMaximaCurso: z
      .number({ message: 'Carga do curso deve ser um número' })
      .positive('Deve ser maior que zero')
      .max(500, 'Máximo de 500 horas'),
    possuiCurricularizacaoExtensao: z.boolean().optional(),
    horasCurricularizacaoExtensao: z
      .number()
      .int()
      .positive('Deve ser maior que zero')
      .optional()
  })
  .superRefine((data, ctx) => {
    if (
      data.possuiCurricularizacaoExtensao === true &&
      !data.horasCurricularizacaoExtensao
    ) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Informe as horas de curricularização.',
        path: ['horasCurricularizacaoExtensao']
      });
    }
  });

export type ActivityFormSchema = z.infer<typeof activitySchema>;
