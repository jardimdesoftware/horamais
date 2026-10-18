#!/usr/bin/env bash
set -Eeuo pipefail

stack_dir=${1:-/home/ubuntu/horamais}
cd "$stack_dir"
mkdir -p .deploy
exec 9>.deploy/lock
flock -n 9 || { echo 'Outro deploy ja esta em andamento'; exit 0; }

git diff --quiet && git diff --cached --quiet || {
  echo 'Checkout de producao tem alteracoes rastreadas; deploy suspenso' >&2
  exit 1
}
git fetch --quiet origin main
revision=$(git rev-parse FETCH_HEAD)
[[ "$revision" =~ ^[a-f0-9]{40}$ ]]
if [[ -f .deploy/current.env ]] && grep -Fxq "DEPLOYED_REVISION=$revision" .deploy/current.env; then
  echo "Revisao $revision ja implantada"
  exit 0
fi

owner=jardimdesoftware
for component in backend frontend; do
  image="ghcr.io/$owner/horas-discentes-$component:sha-$revision"
  if ! manifest=$(docker buildx imagetools inspect "$image" 2>/dev/null); then
    echo "Aguardando imagem aprovada: $image"
    exit 0
  fi
  digest=$(awk '$1 == "Digest:" { print $2; exit }' <<< "$manifest")
  [[ "$digest" =~ ^sha256:[a-f0-9]{64}$ ]] || {
    echo "Digest invalido: $image" >&2
    exit 1
  }
  if [[ "$component" == backend ]]; then
    backend_image="ghcr.io/$owner/horas-discentes-backend@$digest"
  else
    frontend_image="ghcr.io/$owner/horas-discentes-frontend@$digest"
  fi
done

stage=$(mktemp -d "$stack_dir/.deploy/stage-XXXXXXXX")
trap 'rm -rf -- "$stage"' EXIT
for path in docker-compose.yml garage.toml scripts/deploy/deploy-vm.sh; do
  git show "$revision:$path" > "$stage/${path##*/}"
done
bash "$stage/deploy-vm.sh" "$stack_dir" "$stage" "$backend_image" "$frontend_image" "$revision"
git reset --hard "$revision" >/dev/null
echo "Revisao $revision implantada e checkout atualizado"
