#!/usr/bin/env bash
set -Eeuo pipefail

stack_dir=${1:?Diretorio da stack ausente}
stage=${2:?Diretorio de preparo ausente}
backend_image=${3:?Imagem backend ausente}
frontend_image=${4:?Imagem frontend ausente}
revision=${5:?Revisao ausente}

[[ "$stack_dir" =~ ^/[a-zA-Z0-9_./-]+$ && "$stack_dir" != *..* ]] || exit 1
[[ "$revision" =~ ^[a-f0-9]{40}$ ]]
[[ "$backend_image" =~ ^ghcr\.io/[a-z0-9._/-]+/horas-discentes-backend@sha256:[a-f0-9]{64}$ ]]
[[ "$frontend_image" =~ ^ghcr\.io/[a-z0-9._/-]+/horas-discentes-frontend@sha256:[a-f0-9]{64}$ ]]
cd "$stack_dir"
test -f .env && test -f "$stage/docker-compose.yml" && test -f "$stage/garage.toml"
for key in GARAGE_RPC_SECRET GARAGE_ACCESS_KEY GARAGE_SECRET_KEY; do
  grep -Eq "^$key=.+" .env || { echo "Variavel $key ausente" >&2; exit 1; }
done

umask 077
mkdir -p .deploy
rollback_dir=$(mktemp -d "$stack_dir/.deploy/rollback-XXXXXXXX")
cp docker-compose.yml "$rollback_dir/"
for file in docker-compose.override.yml garage.toml; do
  [[ ! -f "$file" ]] || cp "$file" "$rollback_dir/"
done
[[ ! -f .deploy/current.env ]] || cp .deploy/current.env "$rollback_dir/"

old_backend_id=$(docker inspect -f '{{.Image}}' "$(docker compose ps -q backend)" 2>/dev/null || true)
old_backend_ref=$(docker inspect -f '{{.Config.Image}}' "$(docker compose ps -q backend)" 2>/dev/null || true)

compose_new() {
  docker compose --env-file "$stack_dir/.env" --env-file "$stack_dir/.deploy/current.env" \
    -f "$stack_dir/docker-compose.yml" "$@"
}
rollback() {
  trap - ERR
  echo 'Deploy falhou; restaurando a stack anterior' >&2
  cp "$rollback_dir/docker-compose.yml" "$stack_dir/docker-compose.yml"
  if [[ -f "$rollback_dir/docker-compose.override.yml" ]]; then
    cp "$rollback_dir/docker-compose.override.yml" "$stack_dir/docker-compose.override.yml"
  else
    rm -f -- "$stack_dir/docker-compose.override.yml"
  fi
  if [[ -f "$rollback_dir/garage.toml" ]]; then
    cp "$rollback_dir/garage.toml" "$stack_dir/garage.toml"
  else
    rm -f -- "$stack_dir/garage.toml"
  fi
  if [[ -f "$rollback_dir/current.env" ]]; then
    cp "$rollback_dir/current.env" "$stack_dir/.deploy/current.env"
  else
    rm -f -- "$stack_dir/.deploy/current.env"
  fi
  if [[ -n "$old_backend_id" && -n "$old_backend_ref" && "$old_backend_ref" != *@* ]]; then
    docker tag "$old_backend_id" "$old_backend_ref" || true
  fi
  if [[ -f "$rollback_dir/current.env" ]]; then
    compose_new up -d --wait --wait-timeout 240 --pull never --no-build || echo 'Rollback requer intervencao manual' >&2
  else
    docker compose --env-file "$stack_dir/.env" up -d --pull never --no-build || echo 'Rollback requer intervencao manual' >&2
  fi
}
trap rollback ERR

cp "$stage/docker-compose.yml" docker-compose.yml
cp "$stage/garage.toml" garage.toml
rm -f -- docker-compose.override.yml
printf 'BACKEND_IMAGE=%s\nFRONTEND_IMAGE=%s\nDEPLOYED_REVISION=%s\n' \
  "$backend_image" "$frontend_image" "$revision" > .deploy/current.env

compose_new config --quiet
compose_new pull backend frontend garage
compose_new up -d --wait --wait-timeout 240 --remove-orphans
compose_new exec -T backend wget -q -O /dev/null http://127.0.0.1:5000/health
compose_new exec -T frontend node -e "fetch('http://127.0.0.1:3000/').then(r => process.exit(r.ok ? 0 : 1)).catch(() => process.exit(1))"
compose_new exec -T garage /garage status

backend_running=$(docker inspect -f '{{.Config.Image}}' "$(compose_new ps -q backend)")
frontend_running=$(docker inspect -f '{{.Config.Image}}' "$(compose_new ps -q frontend)")
[[ "$backend_running" == "$backend_image" && "$frontend_running" == "$frontend_image" ]]

trap - ERR
echo "Deploy aprovado: revision=$revision"
