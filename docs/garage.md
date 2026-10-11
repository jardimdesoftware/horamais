# Garage (#479)

O Compose usa Garage `v2.4.1` com API S3 interna em `garage:3900`, região
`garage`, bucket `certificados` (configurável por `GARAGE_BUCKET`) e volumes
persistentes separados de metadados e dados. `--single-node --default-bucket`
prepara o layout, a chave e o bucket no primeiro boot. O backend recebe apenas
a chave S3 desse bucket; não recebe token de administração do cluster. O
serviço de armazenamento não publica porta em produção. No desenvolvimento,
a API S3 fica em `127.0.0.1:9002`.

Esta implantação tem **um nó e nenhuma redundância**. Para dados que precisem
ser preservados, faça backup dos dois volumes do Garage e do banco de dados e
teste a restauração. Um cluster de múltiplos nós exige outro layout e outra
configuração.

## Primeiro boot

Acrescente ao `.env` os valores abaixo. Gere-os com uma fonte criptográfica,
mantenha-os fora do Git e não os altere depois do primeiro boot:

```text
GARAGE_RPC_SECRET=<64 caracteres hexadecimais; openssl rand -hex 32>
GARAGE_ACCESS_KEY=<GK seguido de 32 caracteres hexadecimais>
GARAGE_SECRET_KEY=<64 caracteres hexadecimais; openssl rand -hex 32>
GARAGE_BUCKET=certificados
```

Para a chave de acesso, use `GK$(openssl rand -hex 16)` em um shell Linux.
Confirme que as três chaves não estão vazias. Em seguida:

```sh
docker compose up -d garage
docker compose exec garage /garage status
docker compose exec garage /garage bucket info certificados
docker compose up -d
docker compose ps
```

No desenvolvimento, acrescente `-f docker-compose.dev.yml` aos comandos
`docker compose`. Teste upload, download e exclusão de um certificado pela
aplicação. O backend não cria buckets dinamicamente: se o bucket ou a chave
estiverem incorretos, a operação falha sem ampliar privilégios.

## Instalações antigas com MinIO

Esta migração **descarta os objetos antigos** por decisão do projeto. Não há
cópia automática. Depois de implantar a nova stack e verificar que os serviços
estão saudáveis, pare e remova o contêiner MinIO órfão se ele ainda existir.
O volume antigo não é reutilizado pelo Garage. Para liberar espaço, identifique
primeiro o volume certo com `docker volume ls` e `docker volume inspect` e
remova-o explicitamente com `docker volume rm <nome-do-volume-minio>`.
Não use `docker compose down -v`, pois isso também apagaria os volumes do
PostgreSQL e do Garage.

Referências: [início rápido do Garage](https://garagehq.deuxfleurs.fr/documentation/quick-start/)
e [compatibilidade S3](https://garagehq.deuxfleurs.fr/documentation/reference-manual/s3-compatibility/).
