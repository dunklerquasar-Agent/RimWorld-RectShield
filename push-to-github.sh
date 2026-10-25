#!/usr/bin/env bash
# Pusht dieses vorbereitete Repository auf ein leeres GitHub-Repo.
#
#   ./push-to-github.sh https://github.com/DEINNAME/RimWorld-RectShield.git
#   ./push-to-github.sh git@github.com:DEINNAME/RimWorld-RectShield.git
#
# Das Ziel-Repo muss auf GitHub bereits existieren und LEER sein
# (kein README, keine Lizenz, kein .gitignore beim Anlegen auswaehlen).
set -euo pipefail

cd "$(dirname "$0")"

REPO_URL="${1:-}"
if [[ -z "$REPO_URL" ]]; then
  read -r -p "GitHub-Repo-URL (HTTPS oder SSH): " REPO_URL
fi

if [[ -z "$REPO_URL" ]]; then
  echo "Fehler: keine URL angegeben." >&2
  exit 1
fi

if [[ ! -d .git ]]; then
  echo "Initialisiere lokales Git-Repo..."
  git init -b main
  git add -A
  git commit -m "RectShield v0.1 - rectangular shields for RimWorld 1.6/1.7"
fi

if git remote get-url origin >/dev/null 2>&1; then
  git remote set-url origin "$REPO_URL"
else
  git remote add origin "$REPO_URL"
fi

echo "==> Pushe nach $REPO_URL"
git branch --show-current
git push -u origin main

echo ""
echo "Fertig. Naechste Schritte:"
echo "  1. About/About.xml: packageId, author, url auf deine Daten aendern."
echo "  2. .github/workflows/validate.yml prueft bei jedem Push automatisch."
echo "  3. Optional: Secret RW17_MANAGED setzen fuer den Game-Build-Job."
