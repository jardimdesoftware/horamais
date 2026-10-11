#!/usr/bin/env bash
set -Eeuo pipefail

deploy_dir=${1:?Diretorio da stack ausente}
run_id=${2:?Run ID ausente}
backend_image=${3:?Imagem backend ausente}
frontend_image=${4:?Imagem frontend ausente}
revision=${5:?Revisao ausente}
registry_user=${6:?Usuario GHCR ausente}

[[ "$deploy_dir" =~ ^/[a-zA-Z0-9_./-]+$ && "$deploy_dir" != *..* ]] || exit 1
[[ "$run_id" =~ ^[0-9]+$ && "$revision" =~ ^[a-f0-9]{40}$ ]] || exit 1
[[ "$registry_user" =~ ^[a-zA-Z0-9_-]+$ ]] || exit 1
[[ "$backend_image" =~ ^ghcr\.io/[a-z0-9._/-]+/horas-discentes-backend@sha256:[a-f0-9]{64}$ ]] || exit 1
[[ "$frontend_image" =~ ^ghcr\.io/[a-z0-9._/-]+/horas-discentes-frontend@sha256:[a-f0-9]{64}$ ]] || exit 1

cd "$deploy_dir"
test -f .env || { echo '.env de producao ausente' >&2; exit 1; }
staging_dir="$deploy_dir/.incoming-$run_id"
test -f "$staging_dir/docker-compose.yml" && test -f "$staging_dir/garage.toml"

umask 077
auth_dir=$(mktemp -d)
export DOCKER_CONFIG="$auth_dir"
trap 'rm -rf -- "$auth_dir"' EXIT
IFS= read -r registry_token
printf '%s' "$registry_token" | docker login ghcr.io --username "$registry_user" --password-stdin >/dev/null
unset registry_token

if [[ -f .deploy/current.env ]] &&
  grep -Fxq "BACKEND_IMAGE=$backend_image" .deploy/current.env &&
  grep -Fxq "FRONTEND_IMAGE=$frontend_image" .deploy/current.env; then
  echo "Imagens ja implantadas: revision=$revision"
  rm -r -- "$staging_dir"
  exit 0
fi

mkdir -p .deploy
rollback_dir="$deploy_dir/.deploy/rollback-$run_id"
had_previous=false
if [[ -f .deploy/current.env && -f docker-compose.yml && -f garage.toml ]]; then
  mkdir -p "$rollback_dir"
  cp docker-compose.yml garage.toml .deploy/current.env "$rollback_dir/"
  had_previous=true
fi

compose() {
  docker compose --env-file "$deploy_dir/.env" --env-file "$deploy_dir/.deploy/current.env" \
    -f "$deploy_dir/docker-compose.yml" "$@"
}

rollback() {
  trap - ERR
  if [[ "$had_previous" == true ]]; then
    echo 'Deploy falhou; restaurando compose e imagens anteriores' >&2
    cp "$rollback_dir/docker-compose.yml" "$deploy_dir/docker-compose.yml"
    cp "$rollback_dir/garage.toml" "$deploy_dir/garage.toml"
    cp "$rollback_dir/current.env" "$deploy_dir/.deploy/current.env"
    compose up -d --wait --wait-timeout 240 || echo 'Rollback requer intervencao manual' >&2
  else
    echo 'Primeiro deploy falhou; nao havia versao anterior para rollback' >&2
  fi
}
trap rollback ERR

mv "$staging_dir/docker-compose.yml" "$deploy_dir/docker-compose.yml"
mv "$staging_dir/garage.toml" "$deploy_dir/garage.toml"
printf 'BACKEND_IMAGE=%s\nFRONTEND_IMAGE=%s\nDEPLOYED_REVISION=%s\n' \
  "$backend_image" "$frontend_image" "$revision" > .deploy/current.env

compose config --quiet
compose pull backend frontend
compose up -d --wait --wait-timeout 240
compose exec -T backend wget -q -O /dev/null http://127.0.0.1:5000/health
compose exec -T frontend node -e "fetch('http://127.0.0.1:3000/').then(r => process.exit(r.ok ? 0 : 1)).catch(() => process.exit(1))"
compose exec -T garage /garage status

backend_running=$(docker inspect -f '{{.Config.Image}}' "$(compose ps -q backend)")
frontend_running=$(docker inspect -f '{{.Config.Image}}' "$(compose ps -q frontend)")
[[ "$backend_running" == "$backend_image" && "$frontend_running" == "$frontend_image" ]]

trap - ERR
rm -r -- "$staging_dir"
echo "Deploy aprovado: revision=$revision backend=$backend_image frontend=$frontend_image"
