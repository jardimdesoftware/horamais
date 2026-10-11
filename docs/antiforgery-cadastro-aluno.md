# Remoção da rota legada de cadastro de aluno (#491)

O Semgrep da #481 apontou falta de antiforgery em `AlunoController.Criar`,
que atendia `POST /api/Aluno`. O
[relatório original](https://github.com/jardimdesoftware/horamais/actions/runs/38072243642/artifacts/11676528872)
classificou o achado como `WARNING`, não bloqueante.

A action tinha `[AllowAnonymous]`, ignorava o corpo e qualquer identidade e
apenas retornava `410 Gone` com uma mensagem de descontinuação. Não alterava
dados; portanto, aquele código não apresentava uma operação explorável via
CSRF. Como a API não oferece mais cadastro por senha e o frontend chama
`POST /api/auth/google-register`, manter essa rota somente para retornar um
erro não traz benefício ao fluxo atual.

A action e seus atributos foram removidos de `AlunoController`. O teste de
integração agora verifica que `POST /api/aluno` retorna `404 Not Found`, em
vez de `410 Gone`. O teste separado de `POST /api/auth/google-register`
continua cobrindo a rota de cadastro atual. O smoke test da imagem final na
CI também exige `404` para o POST removido em AMD64 e ARM64.

Não foi adicionada nenhuma anotação `nosemgrep` nem desativada a regra. Ao
atualizar a branch da #481 com a `main`, o SAST deve deixar de encontrar
**esse ponto** porque a action não existe mais; os demais endpoints seguem
sujeitos às mesmas regras.
