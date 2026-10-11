# Triagem do aviso de antiforgery no cadastro legado (#491)

O Semgrep da #481 marcou `AlunoController.Criar` com a regra
`csharp.dotnet.security.mvc-missing-antiforgery.mvc-missing-antiforgery`.
O [relatório original](https://github.com/jardimdesoftware/horamais/actions/runs/38072243642/artifacts/11676528872)
classificou o achado como `WARNING`, não bloqueante.

## Comportamento da rota

- Método e caminho: `POST /api/Aluno`, definidos por `[HttpPost]` e
  `[Route("api/[controller]")]`.
- A action tem `[AllowAnonymous]`; não consulta `User`, cookies, token nem
  qualquer parâmetro do pedido.
- O corpo da action apenas retorna `410 Gone` com a mensagem de que o cadastro
  por senha foi descontinuado. Não chama casos de uso, repositórios ou banco e
  não altera estado.
- O cadastro atual ocorre em `POST /api/auth/google-register`. Esta triagem
  cobre somente a rota legada de `AlunoController`.

Em `Program.cs`, a API configura JWT Bearer como esquema padrão de autenticação
e desafio. `AddIdentity` registra suporte a cookies no aplicativo, mas esta
action é anônima e não realiza nenhuma operação dependente de identidade.
Mesmo que um navegador envie cookies automaticamente, a resposta desta rota
é a mesma e não há ação autenticada a induzir. Portanto, a ausência de token
antiforgery **nesta action** não cria um cenário de CSRF explorável no código
atual. Isso não é uma conclusão sobre outras rotas POST.

Foi adicionada uma anotação `nosemgrep` com o identificador completo da regra
imediatamente antes de `[HttpPost]`, acompanhada do motivo. Ela limita a
triagem a este achado; o restante do SAST permanece ativo. Se a rota voltar
a alterar dados ou usar autenticação, a anotação deve ser retirada e a
proteção CSRF reavaliada. O resultado do Semgrep será conferido novamente ao
atualizar a branch da #481 com a `main`.
