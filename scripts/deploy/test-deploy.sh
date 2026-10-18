#!/usr/bin/env bash
set -euo pipefail

root=$(mktemp -d)
trap 'rm -rf -- "$root"' EXIT
mkdir -p "$root/bin" "$root/stack"
export MOCK_STACK="$root/stack" MOCK_FAIL_ONCE="$root/fail-once"

cat > "$root/bin/docker" <<'MOCK'
#!/usr/bin/env bash
set -euo pipefail
case "$1" in
  compose)
    shift
    while [[ "$1" == --env-file || "$1" == -f ]]; do shift 2; done
    case "$1" in
      ps) echo "mock-$3" ;;
      up)
        if [[ -e "$MOCK_FAIL_ONCE" ]]; then
          rm "$MOCK_FAIL_ONCE"
          exit 1
        fi ;;
      config|pull|exec) : ;;
      *) echo "Unexpected compose command: $*" >&2; exit 1 ;;
    esac ;;
  inspect)
    if [[ "$*" == *'{{.Image}}'* ]]; then
      echo sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
    elif [[ -f "$MOCK_STACK/.deploy/current.env" ]]; then
      if [[ "$*" == *frontend* ]]; then
        sed -n 's/^FRONTEND_IMAGE=//p' "$MOCK_STACK/.deploy/current.env"
      else
        sed -n 's/^BACKEND_IMAGE=//p' "$MOCK_STACK/.deploy/current.env"
      fi
    else
      echo ghcr.io/example/horas-discentes-backend:latest
    fi ;;
  tag) : ;;
  *) echo "Unexpected docker command: $*" >&2; exit 1 ;;
esac
MOCK
chmod +x "$root/bin/docker"
export PATH="$root/bin:$PATH"

stack="$root/stack"
cat > "$stack/.env" <<'ENV'
GARAGE_RPC_SECRET=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
GARAGE_ACCESS_KEY=GKaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
GARAGE_SECRET_KEY=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
ENV
printf 'old compose\n' > "$stack/docker-compose.yml"
printf 'old override\n' > "$stack/docker-compose.override.yml"
script="$(cd "$(dirname "$0")" && pwd)/deploy-vm.sh"
sha=cccccccccccccccccccccccccccccccccccccccc
backend_a="ghcr.io/example/horas-discentes-backend@sha256:$(printf 'a%.0s' {1..64})"
frontend_a="ghcr.io/example/horas-discentes-frontend@sha256:$(printf 'b%.0s' {1..64})"
backend_b="ghcr.io/example/horas-discentes-backend@sha256:$(printf 'd%.0s' {1..64})"
frontend_b="ghcr.io/example/horas-discentes-frontend@sha256:$(printf 'e%.0s' {1..64})"

stage() {
  mkdir -p "$stack/.deploy/stage-$1"
  printf 'compose revision %s\n' "$1" > "$stack/.deploy/stage-$1/docker-compose.yml"
  printf 'garage revision %s\n' "$1" > "$stack/.deploy/stage-$1/garage.toml"
}
deploy() {
  bash "$script" "$stack" "$stack/.deploy/stage-$1" "$2" "$3" "$sha"
}

stage 1
touch "$MOCK_FAIL_ONCE"
if deploy 1 "$backend_a" "$frontend_a"; then
  echo 'Expected first deploy failure' >&2
  exit 1
fi
grep -Fxq 'old compose' "$stack/docker-compose.yml"
grep -Fxq 'old override' "$stack/docker-compose.override.yml"
[[ ! -e "$stack/.deploy/current.env" ]]

stage 2
deploy 2 "$backend_a" "$frontend_a"
grep -Fxq "BACKEND_IMAGE=$backend_a" "$stack/.deploy/current.env"
grep -Fxq 'compose revision 2' "$stack/docker-compose.yml"
[[ ! -e "$stack/docker-compose.override.yml" ]]

stage 3
touch "$MOCK_FAIL_ONCE"
if deploy 3 "$backend_b" "$frontend_b"; then
  echo 'Expected update failure' >&2
  exit 1
fi
grep -Fxq "BACKEND_IMAGE=$backend_a" "$stack/.deploy/current.env"
grep -Fxq "FRONTEND_IMAGE=$frontend_a" "$stack/.deploy/current.env"
grep -Fxq 'compose revision 2' "$stack/docker-compose.yml"
grep -Fxq 'garage revision 2' "$stack/garage.toml"
echo 'Primeiro deploy, atualizacao e rollback: OK'
