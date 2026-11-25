#!/usr/bin/env bash
# Crea en GitHub un issue por cada archivo de docs/issues, en orden (01 a 13).
# Requisitos: GitHub CLI (gh) instalado y autenticado (gh auth login).
#
# IMPORTANTE:
# - Ejecútalo UNA sola vez desde la raíz del repo; si lo repites, duplica los issues.
# - Ejecútalo ANTES de abrir cualquier pull request. En GitHub, issues y PRs
#   comparten la numeración; así los issues quedan como #1 a #13.
set -euo pipefail

if [ "$(gh issue list --state all --limit 1 --json number --jq 'length')" != "0" ]; then
  echo "El repositorio ya tiene issues. Se cancela para no duplicarlos."
  exit 1
fi

for f in docs/issues/*.md; do
  title=$(head -n 1 "$f" | sed 's/^# *//')
  tmp=$(mktemp)
  tail -n +2 "$f" > "$tmp"
  echo "Creando: $title"
  gh issue create --title "$title" --body-file "$tmp"
  rm -f "$tmp"
done

echo "Listo. Revisa los issues con: gh issue list"
