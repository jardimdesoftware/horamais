# Deploy automático na VM Oracle

A VM verifica a `main` a cada cinco minutos por um timer do systemd. Os workflows
`Backend CI/CD Workflow` e `Frontend CI/CD Workflow` executam em todo push da
`main` e publicam as duas imagens multiarch com a tag `sha-<commit>` somente
após os gates de segurança. O timer espera as duas tags do mesmo commit,
resolve os digests imutáveis e atualiza a stack. Assim, a VM não recebe uma
conexão do GitHub Actions e nenhuma chave SSH é armazenada no GitHub.

## Instalação inicial

A VM deve ter Docker, Buildx, Compose v2, Git, Bash e `flock`. O usuário `ubuntu`
precisa pertencer ao grupo `docker`, que equivale a acesso privilegiado ao host.
O checkout fica em `/home/ubuntu/horamais` e o `.env` local permanece fora do
Git. Preencha `GARAGE_RPC_SECRET`, `GARAGE_ACCESS_KEY` e `GARAGE_SECRET_KEY`
conforme [Garage](garage.md). Faça backup do banco antes da primeira migração;
os objetos antigos do MinIO não são copiados automaticamente.

Copie os arquivos `poll-vm.sh`, `horamais-deploy.service` e
`horamais-deploy.timer` da revisão aprovada para a VM. O script fica em
`/home/ubuntu/horamais/scripts/deploy/poll-vm.sh` e as unidades em
`/etc/systemd/system/`. Depois:

```sh
sudo systemctl daemon-reload
sudo systemctl enable --now horamais-deploy.timer
sudo systemctl start horamais-deploy.service
sudo systemctl status horamais-deploy.service
```

A regra Oracle de SSH `22/TCP` pode ser independente do Tailscale, mas o timer
não precisa de SSH público para funcionar. Mantenha somente `80/TCP`, `443/TCP`
e `22/TCP` expostos; Postgres, backend e Garage ficam na rede interna do Compose.

## Fluxo e rollback

`poll-vm.sh` usa um lock para impedir duas execuções simultâneas. Se houver
alterações rastreadas no checkout, ele suspende o deploy. Ele busca a `main`,
aguarda as duas tags e prepara o Compose e a configuração do Garage da mesma
revisão. `deploy-vm.sh` salva os arquivos anteriores em `.deploy/rollback-*`,
substitui o antigo override local, sobe os serviços e verifica a saúde do
backend, frontend e Garage. Se qualquer etapa falhar, restaura os arquivos e
as imagens anteriores. Após sucesso, o checkout é atualizado para a revisão
implantada e o MinIO órfão é removido; o volume antigo fica preservado.

O rollback não desfaz migrações do banco nem alterações em volumes. Para mudanças
incompatíveis, prepare backup e plano de restauração antes do merge. O primeiro
corte preserva o Compose antigo, seu override e as imagens locais para que uma
falha de inicialização possa restaurar a stack anterior.

```sh
systemctl list-timers horamais-deploy.timer
journalctl -u horamais-deploy.service -n 100 --no-pager
cd /home/ubuntu/horamais
cat .deploy/current.env
sudo docker compose --env-file .env --env-file .deploy/current.env -f docker-compose.yml ps
```

As referências das imagens e a revisão ficam em `.deploy/current.env`; esse
arquivo não contém credenciais. O `.env` e o backup do banco continuam privados
na VM. O teste `scripts/deploy/test-deploy.sh` exercita o primeiro corte e o
rollback de uma atualização com um Docker simulado.
