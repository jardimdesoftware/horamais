const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const ts = require('typescript');

const source = ts.transpileModule(
  fs.readFileSync(path.join(__dirname, '../src/lib/authOptions.ts'), 'utf8'),
  { compilerOptions: { module: ts.ModuleKind.CommonJS, esModuleInterop: true } }
).outputText;

let auth;
let post;
beforeEach(() => {
  post = jest.fn();
  const exports = {};
  vm.runInNewContext(source, {
    exports,
    process,
    URLSearchParams,
    console: { error: jest.fn() },
    require: (id) => {
      if (id === 'axios') return { post, isAxiosError: (e) => e.isAxiosError };
      if (id.startsWith('next-auth/providers/')) return (options) => options;
      return require(id);
    }
  });
  auth = exports.authOptions;
});

test('novo discente segue ao cadastro sem criar sessão ou receber token', async () => {
  post.mockResolvedValue({ data: {
    requiresRegistration: true, nome: 'Aluno Teste',
    email: 'aluno@discente.ifpe.edu.br', role: null, token: null
  } });
  const user = {};
  const result = await auth.callbacks.signIn({ account: { provider: 'google', id_token: 'test' }, user });
  const redirect = new URL(result, 'http://localhost:3000');
  expect(redirect.pathname).toBe('/primeiroAcesso');
  expect(redirect.searchParams.get('email')).toBe('aluno@discente.ifpe.edu.br');
  expect(user.accessToken).toBeUndefined();
});

test('conta cadastrada recebe o token e perfil do backend', async () => {
  post.mockResolvedValue({ data: { nome: 'Aluno', email: 'a@discente.ifpe.edu.br', role: 'ALUNO', token: 'backend-token', requiresRegistration: false } });
  const user = {};
  expect(await auth.callbacks.signIn({ account: { provider: 'google' }, user })).toBe(true);
  expect(user.accessToken).toBe('backend-token');
  expect(user.role).toBe('aluno');
});

test('domínio recusado redireciona para a tela de e-mail institucional', async () => {
  post.mockRejectedValue({ isAxiosError: true, response: { status: 400, data: { message: 'O primeiro acesso com Google exige um e-mail @discente.ifpe.edu.br.' } } });
  expect(await auth.callbacks.signIn({ account: { provider: 'google' }, user: {} })).toBe('/email-institucional');
  expect(auth.pages.error).toBe('/');
});

test('falha de rede não concede acesso', async () => {
  post.mockRejectedValue({ isAxiosError: true, code: 'ECONNREFUSED' });
  expect(await auth.callbacks.signIn({ account: { provider: 'google' }, user: {} })).toBe('/?error=GoogleLoginFailed');
});

test('login por senha continua independente do Google', async () => {
  expect(await auth.callbacks.signIn({ account: { provider: 'credentials' }, user: {} })).toBe(true);
  expect(post).not.toHaveBeenCalled();
});
