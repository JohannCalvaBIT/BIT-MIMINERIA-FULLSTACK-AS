#!/usr/bin/env bash
# autosave.sh <repo_dir> <rama_remota> [intervalo_s]
# Sube cada N segundos una "foto" del working tree a una rama remota aparte,
# usando comandos de bajo nivel de git: NO toca HEAD, ni el índice, ni la rama
# del agente. Sirve para que el trabajo llegue a GitHub aunque la sesión se
# pause por tope de presupuesto o el agente olvide hacer push.
# Respeta .gitignore. No guarda tokens (no los lee ni los imprime).
REPO="${1:-/workspace/repo}"; BRANCH="${2:-agentsky-autosave/req-01-catalogos}"; EVERY="${3:-180}"
LOG="${AUTOSAVE_LOG:-/tmp/autosave.log}"
cd "$REPO" || exit 1
git config --global --add safe.directory "$REPO" 2>/dev/null || true
IDX="$(mktemp -u /tmp/autosave-idx.XXXXXX)"
while true; do
  (
    export GIT_INDEX_FILE="$IDX"
    git read-tree HEAD 2>/dev/null || git read-tree --empty
    git add -A >/dev/null 2>&1
    tree="$(git write-tree)"
    prev="$(git rev-parse -q --verify refs/autosave/last 2>/dev/null || true)"
    if [ -n "$prev" ] && [ "$(git rev-parse "$prev^{tree}")" = "$tree" ]; then exit 0; fi
    parent="${prev:-$(git rev-parse -q --verify HEAD || true)}"
    args=(); [ -n "$parent" ] && args=(-p "$parent")
    c="$(git -c user.name=agentsky -c user.email=noreply@example.com commit-tree "$tree" "${args[@]}" -m "autosave $(date -u +%FT%TZ)")"
    git update-ref refs/autosave/last "$c"
    if git push -q origin "$c:refs/heads/$BRANCH" >>"$LOG" 2>&1; then echo "$(date -u +%T) push ok $c" >>"$LOG"; else echo "$(date -u +%T) push FALLO" >>"$LOG"; fi
  ) || echo "$(date -u +%T) ciclo con error" >>"$LOG"
  rm -f "$IDX"
  sleep "$EVERY"
done
