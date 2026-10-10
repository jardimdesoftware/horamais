# Gates de segurança no CI/CD

Os workflows de frontend e backend chamam `security.yml` no mesmo commit que
será compilado. Builds e scans de imagem ainda executam se SAST/DAST reprovarem,
para coletar todos os diagnósticos. O envio de imagens, o manifest e a release
dependem dos gates aprovados. Não há deploy
via SSH implementado ainda: a issue #483 deverá preservar essas dependências.

## Ferramentas e política de bloqueio

| Etapa | Cobertura | Condição de bloqueio |
| --- | --- | --- |
| SAST / Semgrep 1.180.0 | C# e TypeScript em `back` e `front`, regras `p/default` | Severidade ERROR/HIGH/CRITICAL ou impacto HIGH/CRITICAL nos metadados da regra |
| DAST / ZAP 2.17.0 | API via OpenAPI e frontend via spider e scan ativo | Risco High (3), maior nível do ZAP; abrange o gate de alta/crítica |
| Trivy 0.70.0 | Pacotes do SO e bibliotecas de cada imagem AMD64/ARM64 | HIGH ou CRITICAL, inclusive sem correção disponível |

Semgrep WARNING/MEDIUM e INFO/LOW não bloqueiam, salvo impacto alto nos
metadados. Não se trata de equivalência universal entre escalas: essa é a
política conservadora adotada pelo projeto. Semgrep usa regras de registro
atualizadas a cada execução, sem upload do código à plataforma e com métricas
desativadas. A análise OSS não substitui revisão de autorização e lógica de negócio.
Na auditoria remota também foram observados checks de Code Quality para C# e
JavaScript/TypeScript. Eles não estão declarados nos workflows locais e sua
configuração administrativa não pôde ser consultada; os gates deste documento
são explícitos nas dependências da publicação.

Erros de execução, timeout, relatório ausente/inválido, ausência de análise de
C# ou TypeScript e falhas de autenticação do DAST bloqueiam a entrega. Nenhum
scanner usa `continue-on-error`. Trivy retorna código 1 para achados bloqueantes;
SAST e DAST também retornam 1 quando a política reprova.

## Ambiente do DAST

`docker-compose.security.yml` cria um projeto exclusivo `horamais-dast`, sem
volumes de produção e com rede interna sem saída para a internet. As imagens
e o scanner ficam nessa rede; somente um proxy de destinos fixos conecta a
rede interna ao runner, com portas publicadas em `127.0.0.1`. As imagens
da aplicação são construídas do checkout atual, com os mesmos Dockerfiles de
produção. As credenciais são aleatórias por execução e não dependem de secrets
de produção. SMTP é capturado pelo Mailpit e o armazenamento usa S3Mock
descartável, somente para testes. Ele não valida permissões/assinaturas S3 e
não substitui a validação do provedor real na migração da issue #479.

O script `scripts/security/dast.mjs` aguarda a aplicação, verifica que a rota
`/api/Curso` rejeita requisição anônima, obtém JWT pelo login real de um admin
de teste e confirma acesso autenticado através do ZAP antes e depois do scan.
O token só é enviado pelo scanner ao backend. A importação OpenAPI alcança
rotas que o spider de páginas não descobriria. Cada scan deve terminar em 100%;
timeout não é considerado aprovação. Containers e volumes são removidos ao final,
com uma etapa adicional de limpeza no workflow.

Limites da cobertura: frontend público e API autenticada como ADMIN. As rotas
de autenticação são excluídas do scan ativo para não revogar a sessão nem seguir
OAuth externo. Não há cobertura de navegação autenticada NextAuth, Google real,
perfis ALUNO/COORDENADOR ou upload de certificados com dados válidos neste DAST.
Os testes de autorização existentes continuam necessários. TLS/domínio da VM
também não são verificados nesta stack HTTP interna.

## Gatilhos e relatórios

Além dos caminhos já monitorados, alterações em workflows, `scripts/security/**`
e `docker-compose.security.yml` disparam as duas esteiras. PRs apenas verificam;
publicação mantém os gatilhos atuais de produção. `security.yml` também pode
ser executado manualmente, sem publicar imagens.

Os artefatos de SAST e DAST contêm resumos sanitizados, sem trechos de código,
evidências de requisição, cookies ou JWTs. O DAST inclui diagnósticos do Compose
com credenciais redigidas. Trivy publica JSON de vulnerabilidades, com scanner
de segredos desabilitado nesse relatório. Retenção: 14 dias. Os uploads executam
mesmo quando há falha; a falha original continua reprovando o job.

## Validação e reprodução

Execute na raiz, com Node.js 22+:

```sh
node --test scripts/security/*.test.mjs
```

Os cenários controlados verificam aprovação sem achados bloqueantes, reprovação
de níveis altos/críticos, rejeição de análise incompleta e sanitização. Para
executar o DAST real, use Docker com containers Linux e Compose v2, com portas
13000, 15000 e 18080 livres:

```sh
node scripts/security/dast.mjs
```

O resultado fica em `artifacts/security/dast-summary.json`. O arquivo
`dast-private.log` é apenas local e não deve ser publicado. O script usa alvos
fixos da stack descartável; não aceita URL de produção. Para repetir o SAST,
use o comando Docker documentado no workflow `security.yml` e depois execute
`node scripts/security/gate.mjs semgrep artifacts/security/semgrep.json artifacts/security/sast-summary.json`.

Antes de concluir a implantação dos gates, confira as execuções reais dos dois
workflows e os respectivos artefatos. Um achado alto/crítico em qualquer etapa
deve impedir os jobs de publicação subsequentes; não desative a verificação para
obter um build verde.

## Proteção de branch e exceções

Um administrador deve configurar em `main` os checks de SAST/DAST e dos builds
com Trivy como obrigatórios, usando os nomes exibidos após a primeira execução.
Os filtros de caminho atuais podem deixar checks pendentes em PRs sem mudanças
de código: ao ativar checks obrigatórios, remova os filtros ou adote um check
agregador sempre executado antes de torná-los obrigatórios. A dependência `needs`
já bloqueia publicação pelos workflows mesmo sem essa proteção de merge.

Não foram alteradas permissões ou regras remotas de branch. A conta usada nesta
implementação tem permissão de push, mas não de administração. A consulta não
retornou proteção clássica de `main`; o ruleset visível estava desabilitado.

Não há exceções de vulnerabilidade configuradas. Comentários `nosemgrep` são
desconsiderados e Trivy usa ignorefile vazio. Qualquer futura exceção exige PR
revisado com regra/CVE, justificativa, responsável, issue de correção e expiração;
não use supressões amplas ou `continue-on-error`.

## Referências

- [Semgrep CLI](https://semgrep.dev/docs/cli-reference)
- [ZAP API](https://www.zaproxy.org/docs/api/)
- [Autenticação externa no ZAP](https://www.zaproxy.org/docs/getting-further/authentication/handling-auth-yourself/)
- [Trivy Action](https://github.com/aquasecurity/trivy-action)
