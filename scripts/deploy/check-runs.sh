#!/usr/bin/env bash
set -euo pipefail

sha=${1:?Informe o commit}
[[ "$sha" =~ ^[a-f0-9]{40}$ ]] || exit 1

main_sha=$(gh api "repos/$GITHUB_REPOSITORY/branches/main" --jq '.commit.sha')
if [[ "$main_sha" != "$sha" ]]; then
  echo "Commit $sha nao e mais a main; deploy antigo ignorado"
  echo 'ready=false' >> "$GITHUB_OUTPUT"
  exit 0
fi

for workflow in production-back.yml production-front.yml; do
  runs=$(gh api -X GET "repos/$GITHUB_REPOSITORY/actions/workflows/$workflow/runs" \
    -f head_sha="$sha" -f event=push -f per_page=10)
  result=$(jq -r --arg sha "$sha" \
    '[.workflow_runs[] | select(.head_sha == $sha)] | first | if . == null then "missing" else "\(.status):\(.conclusion)" end' \
    <<< "$runs")
  echo "$workflow: $result"
  if [[ "$result" != 'completed:success' ]]; then
    echo 'ready=false' >> "$GITHUB_OUTPUT"
    exit 0
  fi
done

echo 'ready=true' >> "$GITHUB_OUTPUT"
