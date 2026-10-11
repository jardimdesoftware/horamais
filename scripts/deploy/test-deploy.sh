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
  login) cat >/dev/null ;;
  compose)
    shift
    while [[ "$1" == --env-file || "$1" == -f ]]; do shift 2; done
    case "$1" in
      up)
        if [[ -e "$MOCK_FAIL_ONCE" ]]; then
          rm "$MOCK_FAIL_ONCE"
          exit 1
        fi ;;
      ps) echo "mock-$3" ;;
      config|pull|exec) : ;;
      *) echo "Unexpected compose command: $*" >&2; exit 1 ;;
    esac ;;
  inspect)
    case "${*: -1}" in
      mock-backend) sed -n 's/^BACKEND_IMAGE=//p' "$MOCK_STACK/.deploy/current.env" ;;
      mock-frontend) sed -n 's/^FRONTEND_IMAGE=//p' "$MOCK_STACK/.deploy/current.env" ;;
      *) exit 1 ;;
    esac ;;
  *) echo "Unexpected docker command: $*" >&2; exit 1 ;;
esac
MOCK
chmod +x "$root/bin/docker"
export PATH="$root/bin:$PATH"

stack="$root/stack"
printf 'PRODUCTION=true\n' > "$stack/.env"
script="$(cd "$(dirname "$0")" && pwd)/deploy-vm.sh"
sha=cccccccccccccccccccccccccccccccccccccccc
backend_a="ghcr.io/example/horas-discentes-backend@sha256:$(printf 'a%.0s' {1..64})"
frontend_a="ghcr.io/example/horas-discentes-frontend@sha256:$(printf 'b%.0s' {1..64})"
backend_b="ghcr.io/example/horas-discentes-backend@sha256:$(printf 'd%.0s' {1..64})"
frontend_b="ghcr.io/example/horas-discentes-frontend@sha256:$(printf 'e%.0s' {1..64})"

stage() {
  mkdir -p "$stack/.incoming-$1"
  printf 'compose revision %s\n' "$1" > "$stack/.incoming-$1/docker-compose.yml"
  printf 'garage revision %s\n' "$1" > "$stack/.incoming-$1/garage.toml"
}
deploy() {
  printf 'test-token\n' | bash "$script" "$stack" "$1" "$2" "$3" "$sha" test-user
}

stage 1
deploy 1 "$backend_a" "$frontend_a"
grep -Fxq "BACKEND_IMAGE=$backend_a" "$stack/.deploy/current.env"
grep -Fxq 'compose revision 1' "$stack/docker-compose.yml"

stage 2
deploy 2 "$backend_a" "$frontend_a"
[[ ! -e "$stack/.incoming-2" ]]
[[ ! -e "$stack/.deploy/rollback-2" ]]

stage 3
touch "$MOCK_FAIL_ONCE"
if deploy 3 "$backend_b" "$frontend_b"; then
  echo 'Expected deploy failure' >&2
  exit 1
fi
grep -Fxq "BACKEND_IMAGE=$backend_a" "$stack/.deploy/current.env"
grep -Fxq "FRONTEND_IMAGE=$frontend_a" "$stack/.deploy/current.env"
grep -Fxq 'compose revision 1' "$stack/docker-compose.yml"
grep -Fxq 'garage revision 1' "$stack/garage.toml"
echo 'Deploy, idempotencia e rollback: OK'
