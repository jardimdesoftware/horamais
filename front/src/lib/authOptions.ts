import type { AuthOptions } from 'next-auth';
import type { JWT } from 'next-auth/jwt';
import CredentialsProvider from 'next-auth/providers/credentials';
import GoogleProvider from 'next-auth/providers/google';

import axios from 'axios';
import jwt_decode from 'jsonwebtoken';

// INTERNAL_API_URL: URL interna do Docker (container -> container)
// NEXT_PUBLIC_API_URL: URL publica baked no bundle pelo Dockerfile
// Fallback: localhost para dev sem Docker
function getApiUrl() {
  return (
    process.env.INTERNAL_API_URL ||
    process.env.NEXT_PUBLIC_API_URL ||
    'http://localhost:5000/api'
  );
}

interface BackendUser {
  nome: string;
  email: string;
  role: 'admin' | 'coordenador' | 'aluno';
  token: string;
}

type GoogleBackendResponse =
  | (BackendUser & { requiresRegistration?: false })
  | { requiresRegistration: true; nome: string; email: string };

interface DecodedToken {
  entidadeId?: string;
  isNewPpc?: string;
  cursoId?: string;
  turmaId?: string;
  [key: string]: unknown;
}

interface ExtendedToken extends JWT {
  accessToken: string;
  role: string;
  entidadeId?: string;
  isNewPpc?: boolean;
  nome?: string;
  cursoId?: string;
  turmaId?: string;
}

export const authOptions: AuthOptions = {
  providers: [
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID!,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET!
    }),
    CredentialsProvider({
      name: 'Credentials',
      credentials: {
        email: { label: 'Email', type: 'text' },
        password: { label: 'Senha', type: 'password' }
      },
      async authorize(credentials) {
        try {
          const apiUrl = getApiUrl();
          const response = await axios.post<BackendUser>(
            `${apiUrl}/auth/login`,
            {
              email: credentials?.email,
              senha: credentials?.password
            }
          );

          const user = response.data;
          const userRole = user.role.toLowerCase();
          return {
            id: user.email,
            sub: user.email,
            name: user.nome,
            email: user.email,
            role: userRole as 'admin' | 'coordenador' | 'aluno',
            accessToken: user.token
          };
        } catch (error) {
          return null;
        }
      }
    })
  ],
  pages: {
    signIn: '/',
    error: '/'
  },
  session: {
    strategy: 'jwt',
    maxAge: 4 * 60 * 60
  },
  secret: process.env.NEXTAUTH_SECRET!,
  callbacks: {
    // Chamado antes do NextAuth criar a sessao. Para o provider do Google,
    // troca o id_token do Google pelo login/JWT do nosso backend - o Google
    // so autentica, quem autoriza o acesso a aplicacao e o backend (o e-mail
    // ainda sem cadastro segue para o primeiro acesso, sem criar sessao).
    async signIn({ account, user }) {
      if (account?.provider !== 'google') {
        return true;
      }

      try {
        const apiUrl = getApiUrl();
        const response = await axios.post<GoogleBackendResponse>(
          `${apiUrl}/auth/google-login`,
          { idToken: account.id_token }
        );

        const backendUser = response.data;
        if (backendUser.requiresRegistration) {
          const params = new URLSearchParams({
            google: '1',
            email: backendUser.email,
            nome: backendUser.nome ?? ''
          });
          return `/primeiroAcesso?${params.toString()}`;
        }
        const userRole = backendUser.role.toLowerCase();

        user.name = backendUser.nome;
        user.email = backendUser.email;
        (user as any).role = userRole;
        (user as any).accessToken = backendUser.token;

        return true;
      } catch (error) {
        if (axios.isAxiosError(error)) {
          const message = error.response?.data?.message;
          const reasons: Record<string, string> = {
            'O primeiro acesso com Google exige um e-mail @discente.ifpe.edu.br.':
              'GoogleDomainNotAllowed',
            'Nenhuma conta encontrada para este e-mail. Faça o cadastro antes de entrar com o Google.':
              'GoogleAccountNotFound',
            'E-mail não verificado. Verifique sua caixa de entrada para confirmar o código de cadastro.':
              'GoogleEmailNotConfirmed',
            'Aluno inativo. Acesso não permitido.': 'GoogleAccountInactive',
            'Token do Google inválido ou expirado.': 'GoogleTokenInvalid'
          };
          const reason =
            typeof message === 'string' ? reasons[message] : undefined;
          // Nunca registrar o erro Axios completo: ele contém o ID token.
          console.error('Falha no login com Google', {
            status: error.response?.status,
            code: error.code,
            reason: reason ?? 'GoogleLoginFailed'
          });
          if (reason === 'GoogleDomainNotAllowed') {
            return '/email-institucional';
          }
          return `/?error=${reason ?? 'GoogleLoginFailed'}`;
        }
        return '/?error=GoogleLoginFailed';
      }
    },

    async jwt({ token, user }) {
      if (user && 'accessToken' in user) {
        const decoded = jwt_decode.decode(
          user.accessToken as string
        ) as DecodedToken;

        token.name = user.name;
        token.email = user.email;
        token.role = user.role;
        token.accessToken = user.accessToken as string;
        if (user.role === 'aluno') {
          token.entidadeId = decoded?.entidadeId as string;
          token.isNewPpc = decoded?.isNewPpc === 'true';
          token.cursoId = decoded?.cursoId as string;
          token.turmaId = decoded?.turmaId as string;
        }
        if (user.role === 'coordenador') {
          token.entidadeId = decoded?.entidadeId as string;
          token.cursoId = decoded?.cursoId as string;
        }
      }

      return token as ExtendedToken;
    },

    async session({ session, token }) {
      session.user.name = token.name as string;
      session.user.email = token.email as string;
      session.user.role = token.role as 'admin' | 'coordenador' | 'aluno';

      (session.user as any).entidadeId = token.entidadeId;
      (session.user as any).isNewPpc = token.isNewPpc;
      (session as any).token = token.accessToken;

      if (token.role === 'aluno') {
        (session.user as any).cursoId = token.cursoId;
        (session.user as any).turmaId = token.turmaId;
      }
      if (token.role === 'coordenador') {
        (session.user as any).cursoId = token.cursoId;
      }
      return session;
    }
  }
};
