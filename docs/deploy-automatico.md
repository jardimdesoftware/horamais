# Deploy automático na VM

O workflow `Production deployment` é disparado quando terminam os workflows
`Backend CI/CD Workflow` ou `Frontend CI/CD Workflow`. Ele só prossegue quando
**ambos os runs de `push` do mesmo commit da `main`** terminaram com sucesso.
Esses runs já incluem SAST, DAST, Trivy e publicação das imagens AMD64/ARM64.
Cada publicação gera uma tag `sha-<commit>`; o deploy resolve os digests dessas
tags e grava as referências imutáveis das duas imagens em `.deploy/current.env`
na VM. Uma revisão antiga é ignorada quando a `main` avança.

## Pré-requisitos na VM

1. Instalar Docker com o plugin Compose v2 e disponibilizar `bash`, `ssh` e
   `scp`. O usuário SSH precisa acessar o daemon Docker e escrever no diretório
   da stack. Use uma conta dedicada com acesso restrito à VM; acesso ao daemon
   Docker equivale, na prática, a privilégios elevados no host.
2. Criar um diretório absoluto da stack, acessível ao usuário SSH, com `.env`
   de produção preenchido conforme [`.env.example`](../.env.example). Manter
   `.env`, volumes do Postgres e do Garage na VM; o workflow não transfere
   segredos da aplicação nem dados persistidos. Planejar backup desses volumes
   antes de ativar o deploy. Configurar o Garage antes do primeiro corte.
3. Garantir saída da VM para `ghcr.io` e acesso da VM ao GitHub Actions por SSH.
   Se os pacotes GHCR forem privados, permitir que o `GITHUB_TOKEN` deste
   repositório os leia. O token é usado via stdin e o login Docker é criado em
   um diretório temporário removido após cada execução.
4. Instalar a chave pública SSH de deploy em `authorized_keys` da conta da VM.
   Conferir a chave pública do host por canal confiável antes de adicioná-la
   ao secret `DEPLOY_KNOWN_HOSTS`; não usar `ssh-keyscan` sem validação.

## Configuração no GitHub

Criar o environment `production`. Nele, definir estas *variables*:

| Variable | Valor |
| --- | --- |
| `DEPLOY_HOST` | Host DNS ou IPv4 da VM, sem esquema ou porta |
| `DEPLOY_USER` | Usuário SSH dedicado |
| `DEPLOY_PORT` | Porta SSH numérica |
| `DEPLOY_PATH` | Caminho absoluto do diretório da stack, sem espaços |

Definir os *secrets* `DEPLOY_SSH_KEY` (chave privada SSH) e
`DEPLOY_KNOWN_HOSTS` (linha validada de `known_hosts`, incluindo porta no
formato `[host]:porta` quando não for 22). No nível do **repositório**, definir
`DEPLOY_ENABLED=true` apenas após o provisionamento e uma validação manual
inicial. Antes disso, o job de deploy fica desabilitado. O environment pode
exigir aprovação de um responsável conforme a política da equipe.

O workflow faz o upload apenas do `docker-compose.yml`, `garage.toml` e do
script de deploy para `.incoming-<run-id>`. A VM lê seu `.env` local. O script
salva o Compose, a configuração do Garage e `.deploy/current.env` anteriores
em `.deploy/rollback-<run-id>`, sobe os digests aprovados e espera a saúde de
todos os serviços. Depois verifica o endpoint `/health` do backend, a resposta
HTTP do frontend, o status do Garage e as imagens efetivas dos containers. Se
qualquer etapa falhar, restaura a configuração e as imagens anteriores e tenta
subi-las. O run fica vermelho mesmo quando o rollback funciona.

## Operação e limites

Use sempre **os dois arquivos de ambiente** ao operar a stack manualmente:

```bash
cd /caminho/da/stack
docker compose --env-file .env --env-file .deploy/current.env -f docker-compose.yml ps
docker compose --env-file .env --env-file .deploy/current.env -f docker-compose.yml logs --tail=100 backend frontend
```

O rollback automático restaura Compose e imagens, mas **não desfaz migrações
do banco nem mudanças em volumes**. Antes de uma migração incompatível, preparar
backup e plano de reversão específicos. No primeiro deploy não existe versão
anterior para restaurar; uma falha exige intervenção manual. Confira os logs do
run e o estado da VM antes de repetir. Cada push na `main` executa os dois
workflows de produção, inclusive quando apenas um componente mudou, para
garantir que as duas imagens pertençam ao mesmo commit.

O teste `scripts/deploy/test-deploy.sh` exercita sucesso, idempotência e
rollback com um Docker simulado. A validação real de conectividade SSH, acesso
ao GHCR, saúde da aplicação e restauração na VM depende da task de
provisionamento da VM. Mantenha `DEPLOY_ENABLED` desabilitado até essa etapa.
