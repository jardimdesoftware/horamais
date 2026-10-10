# Contêineres da aplicação sem root

As imagens finais do backend e do frontend executam como usuários sem
privilégios. Os comandos de instalação continuam no build, antes de `USER`.
O backend usa o usuário `app` da imagem .NET 8 (`APP_UID`, atualmente 1654);
o frontend usa o usuário `node` da imagem Node. Os workflows verificam o UID
efetivo nas imagens AMD64 e ARM64 e a escrita nos diretórios necessários.

## Atualização de uma instalação existente

O mesmo volume `backend_data_protection_keys` continua sendo usado. Apenas o
ponto de montagem muda de `/root/.aspnet/DataProtection-Keys` para
`/home/app/.aspnet/DataProtection-Keys`. Em instalações antigas, os arquivos
do volume podem pertencer a root. Ajuste sua propriedade **antes** de iniciar
o novo backend; não apague nem recrie o volume, pois ele guarda as chaves já
emitidas pela aplicação.

Na VM Linux, no diretório da stack:

1. Pare o backend e identifique o nome completo do volume, normalmente
   `<projeto>_backend_data_protection_keys`:

   ```sh
   docker compose stop backend
   docker volume ls --format '{{.Name}}' | grep 'backend_data_protection_keys$'
   ```

2. Substitua o valor abaixo pelo nome encontrado e confirme que o volume
   existe. Crie uma cópia de segurança local, acessível apenas ao operador:

   ```sh
   VOLUME='<nome-completo-do-volume>'
   docker volume inspect "$VOLUME" >/dev/null
   umask 077
   mkdir -p backups
   chmod 700 backups
   docker run --rm \
     --mount "type=volume,src=$VOLUME,dst=/keys,readonly" \
     --mount "type=bind,src=$PWD/backups,dst=/backup" \
     busybox:1.36 sh -c 'tar -C /keys -cf /backup/backend-data-protection-keys.tar .'
   chmod 600 backups/backend-data-protection-keys.tar
   ```

3. Transfira a propriedade dos arquivos existentes para o UID/GID do usuário
   `app`. A imagem .NET 8 usada neste repositório define ambos como 1654;
   confirme esse valor na imagem que será implantada se a base for alterada:

   ```sh
   docker run --rm \
     --mount "type=volume,src=$VOLUME,dst=/keys" \
     busybox:1.36 sh -c 'chown -R 1654:1654 /keys'
   docker run --rm --user 1654:1654 \
     --mount "type=volume,src=$VOLUME,dst=/keys" \
     busybox:1.36 sh -c 'test -r /keys && test -w /keys'
   ```

4. Implante as novas imagens e o Compose atualizado. Confirme que o backend
   inicia, consegue usar as chaves existentes e não registra avisos de chave
   efêmera ou erros de permissão. Mantenha o backup fora do repositório e
   proteja-o como segredo. Em uma instalação nova, o volume vazio recebe as
   permissões do diretório preparado na imagem e não precisa desta migração.

## Desenvolvimento e verificação

O Compose de desenvolvimento mantém o bind mount `./logs/backend:/app/logs`.
Em hosts Linux, o diretório do host deve permitir escrita ao UID 1654. Se já
existir com propriedade de root, ajuste **somente** esse diretório antes de
subir a stack:

```sh
mkdir -p logs/backend
sudo chown -R 1654:1654 logs/backend
```

Para conferir os usuários e os diretórios graváveis com as imagens em execução:

```sh
docker compose exec backend sh -c 'id; test -w /app/logs; test -w /home/app/.aspnet/DataProtection-Keys'
docker compose exec frontend sh -c 'id; test -w /app/.next/cache'
```

O UID mostrado por `id -u` deve ser diferente de zero em ambos os serviços.
O backend também precisa responder normalmente e manter as chaves de Data
Protection entre reinícios; o frontend deve servir páginas sem erros de cache.
