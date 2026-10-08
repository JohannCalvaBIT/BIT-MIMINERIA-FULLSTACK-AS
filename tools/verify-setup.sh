#!/usr/bin/env bash
# verify-setup.sh — comprueba el entorno del sandbox y deja un reporte legible.
# No cambia nada. Uso: bash tools/verify-setup.sh
ok=0; bad=0
chk() { local n="$1"; shift; local out rc; out=$("$@" 2>&1); rc=$?; out=$(printf '%s' "$out" | head -n1)
  if [ "$rc" -eq 0 ]; then echo "OK   $n: $out"; ok=$((ok+1)); else echo "FALLA $n (rc=$rc): $out"; bad=$((bad+1)); fi; }
chk "git"        git --version
chk "jq"         jq --version
chk "bit(stub)"  bit --version
chk "dotnet"     dotnet --version
chk "node"       node -v
chk "pnpm"       pnpm -v
chk "usuario"    id -un
chk "red github" bash -c 'curl -sS -o /dev/null -w "%{http_code}" -m 10 https://github.com'
chk "red nuget"  bash -c 'curl -sS -o /dev/null -w "%{http_code}" -m 10 https://api.nuget.org/v3/index.json'
chk "red npm"    bash -c 'curl -sS -o /dev/null -w "%{http_code}" -m 10 https://registry.npmjs.org/pnpm'
echo "--- extras informativos (no cuentan)"
command -v docker >/dev/null && echo "docker: presente" || echo "docker: ausente"
ls -d ~/.codex 2>/dev/null && ls ~/.codex | head || echo "~/.codex: ausente"
{ [ -f /tmp/autosave.pid ] && kill -0 "$(cat /tmp/autosave.pid)" 2>/dev/null; } && echo "autosave: corriendo (pid $(cat /tmp/autosave.pid))" || echo "autosave: NO corre"
[ -f /tmp/autosave.log ] && tail -n3 /tmp/autosave.log
echo "RESUMEN ok=$ok falla=$bad"
