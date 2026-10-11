#!/usr/bin/env bash
set -euo pipefail

export GARAGE_RPC_SECRET=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
export GARAGE_ACCESS_KEY=GKaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
export GARAGE_SECRET_KEY=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
export GARAGE_BUCKET=certificados
export AWS_ACCESS_KEY_ID="$GARAGE_ACCESS_KEY"
export AWS_SECRET_ACCESS_KEY="$GARAGE_SECRET_KEY"
export AWS_DEFAULT_REGION=garage
export AWS_EC2_METADATA_DISABLED=true

compose=(docker compose -f docker-compose.dev.yml)
workdir=$(mktemp -d)
trap '"${compose[@]}" down -v >/dev/null 2>&1 || true; rm -rf "$workdir"' EXIT

"${compose[@]}" up -d garage
for attempt in {1..30}; do
  if "${compose[@]}" exec -T garage /garage status >/dev/null 2>&1 &&
    aws --endpoint-url http://127.0.0.1:9002 s3api head-bucket --bucket "$GARAGE_BUCKET" >/dev/null 2>&1; then
    break
  fi
  if (( attempt == 30 )); then
    "${compose[@]}" logs garage
    exit 1
  fi
  sleep 2
done

printf 'garage-smoke\n' > "$workdir/input.txt"
aws --endpoint-url http://127.0.0.1:9002 s3api put-object \
  --bucket "$GARAGE_BUCKET" --key ci/smoke.txt --body "$workdir/input.txt" >/dev/null
aws --endpoint-url http://127.0.0.1:9002 s3api get-object \
  --bucket "$GARAGE_BUCKET" --key ci/smoke.txt "$workdir/output.txt" >/dev/null
cmp "$workdir/input.txt" "$workdir/output.txt"
aws --endpoint-url http://127.0.0.1:9002 s3api delete-object \
  --bucket "$GARAGE_BUCKET" --key ci/smoke.txt >/dev/null
if aws --endpoint-url http://127.0.0.1:9002 s3api head-object \
  --bucket "$GARAGE_BUCKET" --key ci/smoke.txt >/dev/null 2>&1; then
  echo 'Objeto ainda existe apos exclusao' >&2
  exit 1
fi
echo 'Garage S3: upload, download e exclusao aprovados'
