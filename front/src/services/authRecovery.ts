import api from './api';

export interface ForgotPasswordRequest {
  email: string;
}
export interface ForgotPasswordResponse {
  message: string;
}

export interface ValidateCodeRequest {
  email: string;
  code: string;
}
export interface ValidateCodeResponse {
  valid: boolean;
  message: string;
}

export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
}
export interface ResetPasswordResponse {
  message: string;
}

export const forgotPassword = async (
  data: ForgotPasswordRequest
): Promise<ForgotPasswordResponse> => {
  const res = await api.post<ForgotPasswordResponse>(
    '/Auth/forgot-password',
    data
  );
  return res.data;
};

export const validateResetCode = async (
  data: ValidateCodeRequest
): Promise<ValidateCodeResponse> => {
  const res = await api.post<ValidateCodeResponse>('/Auth/validate-code', data);
  return res.data;
};

export const resetPassword = async (
  data: ResetPasswordRequest
): Promise<ResetPasswordResponse> => {
  const res = await api.post<ResetPasswordResponse>(
    '/Auth/reset-password',
    data
  );
  return res.data;
};

export interface ConfirmEmailRequest {
  email: string;
  code: string;
}
export interface ConfirmEmailResponse {
  message: string;
}

export interface ResendVerificationRequest {
  email: string;
}
export interface ResendVerificationResponse {
  message: string;
}

// Confirma o e-mail de um cadastro pendente com o código de 6 dígitos
export const confirmEmail = async (
  data: ConfirmEmailRequest
): Promise<ConfirmEmailResponse> => {
  const res = await api.post<ConfirmEmailResponse>('/Auth/confirm-email', data);
  return res.data;
};

// Reenvia o código de verificação de e-mail
export const resendVerification = async (
  data: ResendVerificationRequest
): Promise<ResendVerificationResponse> => {
  const res = await api.post<ResendVerificationResponse>(
    '/Auth/resend-verification',
    data
  );
  return res.data;
};
