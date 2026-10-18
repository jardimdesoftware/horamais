# Administração de usuários

No ambiente do backend, configure:

```dotenv
INITIAL_ADMIN_EMAIL=mlcr1@discente.ifpe.edu.br
```

Recrie o container do backend para carregar a variável. O e-mail completo deve
pertencer a `ifpe.edu.br` ou `discente.ifpe.edu.br`. A variável não concede
acesso sem autenticação: a ativação ocorre no primeiro login Google desse
endereço, após validar assinatura, audiência e confirmação do e-mail.
Não é necessário cadastrar senha nem preencher cadastro de aluno para essa
conta inicial. Até esse login, a administração inicial fica pendente.

A migração adiciona `UserAccesses` (suspensão administrativa) e
`AdminInitializations` (registro permanente da ativação inicial). A segunda
tabela impede uma nova promoção automática após rebaixamento, reinício,
remoção ou troca da variável. Não apague esse registro para transferir acesso.
Contas e cargos existentes são preservados. O seed antigo de `ADMIN_EMAIL` e
`ADMIN_PASSWORD` continua compatível apenas quando não há `INITIAL_ADMIN_EMAIL`
nem registro da ativação inicial; não o use para novas instalações.

## Transferir a administração

1. Entre com a conta inicial e abra **Usuários** no menu de administração.
2. O responsável definitivo entra/cadastra sua própria conta institucional.
3. Localize a conta ativa com e-mail verificado e clique em **Promover a ADMIN**.
4. Confirme que o responsável consegue entrar como administrador.
5. Na sua conta, clique em **Rebaixar** e confirme. Sua sessão será encerrada.

O rebaixamento restaura o perfil acadêmico vinculado (coordenador ou aluno).
Sem vínculo acadêmico, a conta passa a **Usuário**, com a página **Minha conta**
e sem acesso administrativo. Não cria vínculo acadêmico fictício.

## Regras de acesso

- A tela `/usuarios` e os endpoints `GET /api/usuarios` e
  `PATCH /api/usuarios/{id}` são exclusivos de ADMIN.
- O ADMIN pode atribuir ou editar a coordenação de uma conta institucional já
  cadastrada pela ação **Gerenciar → Atribuir coordenação**. Também pode usar
  **Adicionar coordenador** para informar o e-mail de uma pessoa ainda sem
  conta. Informe nome, curso, número da portaria e DOU. As operações usam
  `PUT /api/usuarios/{id}/coordenador` e `POST /api/usuarios/coordenador`.
  A pessoa entra com Google; o e-mail é verificado nesse primeiro acesso.
  Dados acadêmicos existentes são preservados e sessões antigas são revogadas.
- O administrador pode ativar/desativar qualquer conta e gerenciar o cargo
  ADMIN. A promoção exige e-mail institucional verificado e conta ativa.
- O último ADMIN ativo e verificado não pode ser rebaixado ou desativado.
  A validação é serializada no PostgreSQL, inclusive entre instâncias da API.
- Antes de excluir uma conta ADMIN por um cadastro acadêmico, é necessário
  rebaixá-la pela tela de usuários. Cadastros acadêmicos não alteram suas
  credenciais.
- Alterar cargo/status revoga os tokens existentes; o usuário deve entrar
  novamente. A suspensão administrativa também bloqueia login com Google.
- Desativar não exclui os dados. O bloqueio temporário por tentativas de senha
  continua separado da suspensão administrativa.

O valor local da variável não configura automaticamente a VM ou os secrets do
GitHub Actions. A implantação deve receber a variável no ambiente do backend.
