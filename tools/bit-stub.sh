#!/usr/bin/env bash
# bit-stub.sh v0.2 — reemplazo de pruebas del binario `bit` (BIT Developer Framework)
# para correr /opsx:apply y /opsx:archive dentro de un sandbox de AgentSky.
#
# QUÉ HACE (solo lo que los skills apply/archive llaman):
#   --version, status, instructions apply, exec start, exec end, store list,
#   usage ingest.
# QUÉ NO HACE (falla con mensaje claro, no inventa respuesta):
#   list, validate, doctor, new change, instructions <artifact>.
#   -> pasar SIEMPRE el nombre del change; propose se corre con el bit real.
#
# MÉTRICAS: escribe docs/metricas/execution_log.ndjson con el mismo formato
# que el bit real ({"v":1,"evento":"start|end",...}) y timestamps REALES del
# sandbox. Añade el campo "fuente":"bit-stub" para distinguirlo.
# La forma exacta del JSON de `status` del bit real es [Dato no disponible en
# las fuentes] salvo los campos que los skills nombran; el resto es inferido.

set -euo pipefail

ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
cd "$ROOT"

OPENSPEC_ROOT="${OPENSPEC_ROOT:-openspec}"
CHANGES_DIR="$OPENSPEC_ROOT/changes"
EXEC_LOG="${BIT_EXEC_LOG:-docs/metricas/execution_log.ndjson}"
ACTIVITY_LOG="docs/metricas/guardrail_activity_log.csv"
STATE_DIR="${BIT_STUB_STATE_DIR:-${TMPDIR:-/tmp}/bit-stub-state}"
STUB_USER="${BIT_STUB_USER:-agentsky-sandbox}"

err() { echo "bit (stub): $*" >&2; }

json_escape() {
  local s="$1"
  s="${s//\\/\\\\}"; s="${s//\"/\\\"}"
  s="${s//$'\n'/\\n}"; s="${s//$'\r'/}"; s="${s//$'\t'/\\t}"
  printf '%s' "$s"
}

csv_field() {
  local s="$1"
  s="${s//$'\n'/ }"; s="${s//$'\r'/}"; s="${s//\"/\"\"}"
  printf '"%s"' "$s"
}

now_iso()   { date -u +%Y-%m-%dT%H:%M:%S.%3NZ; }
now_epoch() { date -u +%s; }
new_id() {
  cat /proc/sys/kernel/random/uuid 2>/dev/null || uuidgen 2>/dev/null || echo "stub-$(date +%s%N)"
}

find_change_dir() {
  local dir="$CHANGES_DIR/$1"
  [[ -n "$1" && -d "$dir" ]] && { echo "$dir"; return 0; }
  return 1
}

count_tasks() {
  local f="$1" total=0 done_count=0
  if [[ -f "$f" ]]; then
    total=$(grep -cE '^[[:space:]]*-[[:space:]]*\[[ xX]\]' "$f" 2>/dev/null || true)
    done_count=$(grep -cE '^[[:space:]]*-[[:space:]]*\[[xX]\]' "$f" 2>/dev/null || true)
  fi
  echo "${total:-0} ${done_count:-0}"
}

json_array_of_strings() { # imprime ["a","b"] a partir de args
  local out="" first=1 x
  for x in "$@"; do
    [[ $first -eq 1 ]] && first=0 || out+=","
    out+="\"$(json_escape "$x")\""
  done
  printf '[%s]' "$out"
}

state_file() { echo "$STATE_DIR/${1}__${2}.state"; }

# ---------- subcomandos ----------

cmd_version() { echo "bit (stub) 0.2.1 — reemplazo de pruebas, no es el binario real de BDF"; }

cmd_exec_start() {
  # bit exec start --command <c> --change <n> --tool <t> [--model <m>]
  local command="" change="" tool="" model=""
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --command) command="${2:-}"; shift 2 ;;
      --change)  change="${2:-}";  shift 2 ;;
      --tool)    tool="${2:-}";    shift 2 ;;
      --model)   model="${2:-}";   shift 2 ;;
      *) shift ;;
    esac
  done
  {
    local id ts epoch
    id="$(new_id)"; ts="$(now_iso)"; epoch="$(now_epoch)"
    mkdir -p "$STATE_DIR" "$(dirname "$EXEC_LOG")"
    echo "$id $epoch" > "$(state_file "$command" "$change")"
    printf '{"v":1,"evento":"start","id":"%s","comando":"%s","change":"%s","herramienta":"%s","modelo":"%s","usuario":"%s","fuente":"bit-stub","ts":"%s"}\n' \
      "$id" "$(json_escape "$command")" "$(json_escape "$change")" \
      "$(json_escape "${tool:-desconocida}")" "$(json_escape "${model:-desconocido}")" \
      "$(json_escape "$STUB_USER")" "$ts" >> "$EXEC_LOG"
  } || err "no se pudo registrar exec start (no bloquea)"
  exit 0
}

cmd_exec_end() {
  local command="" change="" result="" log_activity=0
  local role="" activity="" guardrail="" estimated_min="0"
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --command) command="${2:-}"; shift 2 ;;
      --change) change="${2:-}"; shift 2 ;;
      --result) result="${2:-}"; shift 2 ;;
      --log-activity) log_activity=1; shift ;;
      --role) role="${2:-}"; shift 2 ;;
      --activity) activity="${2:-}"; shift 2 ;;
      --guardrail) guardrail="${2:-}"; shift 2 ;;
      --estimated-min) estimated_min="${2:-0}"; shift 2 ;;
      *) shift ;;
    esac
  done
  {
    local id="" start_epoch="" sf end_epoch ts
    sf="$(state_file "$command" "$change")"
    if [[ -f "$sf" ]]; then read -r id start_epoch < "$sf" || true; fi
    end_epoch="$(now_epoch)"; ts="$(now_iso)"
    mkdir -p "$(dirname "$EXEC_LOG")"
    local id_json="null"; [[ -n "$id" ]] && id_json="\"$id\""
    printf '{"v":1,"evento":"end","id":%s,"comando":"%s","change":"%s","resultado":"%s","fuente":"bit-stub","ts":"%s"}\n' \
      "$id_json" "$(json_escape "$command")" "$(json_escape "$change")" "$(json_escape "$result")" "$ts" >> "$EXEC_LOG"

    if [[ "$log_activity" -eq 1 ]]; then
      mkdir -p "$(dirname "$ACTIVITY_LOG")"
      if [[ ! -f "$ACTIVITY_LOG" ]]; then
        echo "fecha,proyecto,template_id,rol,actividad,codigo_guardrail,herramienta,tiempo_estimado_min,tiempo_real_min,ahorro_min,resultado,observaciones" > "$ACTIVITY_LOG"
      fi
      local real_min="" ahorro=""
      if [[ -n "$start_epoch" ]]; then
        real_min=$(awk -v s="$start_epoch" -v e="$end_epoch" 'BEGIN{printf "%.1f",(e-s)/60}')
        ahorro=$(awk -v est="$estimated_min" -v r="$real_min" 'BEGIN{printf "%.1f",est-r}')
      fi
      local tool="${BIT_TOOL:-codex}"
      echo "$(date -u +%F),BIT-MIMINERIA,web,$(csv_field "$role"),$(csv_field "${activity:-$command $change}"),$(csv_field "$guardrail"),${tool},${estimated_min},${real_min},${ahorro},$(csv_field "$result"),$(csv_field "fuente=bit-stub;cambio=${change};estimado_por_agente;real=pared_exec_start_a_end_del_stub")" >> "$ACTIVITY_LOG"
    fi
    rm -f "$sf"
  } || err "no se pudo registrar exec end (no bloquea)"
  exit 0
}

cmd_store_list() { echo "[]"; }

spec_paths() { # lista specs/*/spec.md del change
  local dir="$1" f
  for f in "$dir"/specs/*/spec.md; do [[ -f "$f" ]] && echo "$f"; done
}

cmd_status() {
  local change="" as_json=0
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --change) change="${2:-}"; shift 2 ;;
      --json) as_json=1; shift ;;
      *) shift ;;
    esac
  done
  local dir
  if ! dir=$(find_change_dir "$change"); then
    if [[ "$as_json" -eq 1 ]]; then echo "{\"error\":\"change_not_found\",\"change\":\"$(json_escape "$change")\"}"
    else err "No se encontró el cambio '$change' en $CHANGES_DIR (el stub no soporta 'list': pasa el nombre del change)"; fi
    exit 1
  fi
  local total done_count
  read -r total done_count <<< "$(count_tasks "$dir/tasks.md")"
  local state="in_progress"
  [[ "$total" -eq 0 ]] && state="ready"
  [[ "$total" -gt 0 && "$done_count" -eq "$total" ]] && state="all_done"

  if [[ "$as_json" -eq 0 ]]; then
    echo "Cambio: $change"; echo "Carpeta: $dir"
    echo "Tareas: $done_count/$total completadas"; echo "Estado inferido: $state"
    return 0
  fi

  local hp="missing" hd="missing" ht="missing" hs="missing"
  [[ -f "$dir/proposal.md" ]] && hp="done"
  [[ -f "$dir/design.md" ]] && hd="done"
  [[ -f "$dir/tasks.md" ]] && ht="done"
  local specs=(); mapfile -t specs < <(spec_paths "$dir")
  [[ ${#specs[@]} -gt 0 ]] && hs="done"

  cat <<JSON
{
  "schemaName": "spec-driven",
  "planningHome": {"root": "$(json_escape "$OPENSPEC_ROOT")", "changesDir": "$(json_escape "$CHANGES_DIR")"},
  "changeRoot": "$(json_escape "$dir")",
  "actionContext": {"mode": "local", "allowedEditRoots": ["."]},
  "artifactPaths": {"specs": {"existingOutputPaths": $(json_array_of_strings "${specs[@]}")}},
  "state": "$state",
  "progress": {"total": $total, "complete": $done_count},
  "artifacts": [
    {"id": "proposal", "status": "$hp"},
    {"id": "specs", "status": "$hs"},
    {"id": "design", "status": "$hd"},
    {"id": "tasks", "status": "$ht"}
  ],
  "_stub_warning": "JSON inferido por bit-stub desde carpetas y tasks.md. Solo los campos que los skills nombran (planningHome.changesDir, changeRoot, artifactPaths.specs.existingOutputPaths, actionContext.mode, artifacts[].status) tienen forma esperada; el resto no proviene del bit real."
}
JSON
}

cmd_instructions_apply() {
  local change=""
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --change) change="${2:-}"; shift 2 ;;
      *) shift ;;
    esac
  done
  local dir
  if ! dir=$(find_change_dir "$change"); then echo "{\"error\":\"change_not_found\"}"; exit 1; fi

  local next_task=""
  if [[ -f "$dir/tasks.md" ]]; then
    next_task=$(grep -E '^[[:space:]]*-[[:space:]]*\[ \]' "$dir/tasks.md" 2>/dev/null | head -n1 | sed 's/^[[:space:]]*-[[:space:]]*\[ \][[:space:]]*//' | tr -d '\r' || true)
  fi
  local total done_count
  read -r total done_count <<< "$(count_tasks "$dir/tasks.md")"
  local state="in_progress"
  [[ "$total" -eq 0 ]] && state="ready"
  [[ "$total" -gt 0 && "$done_count" -eq "$total" ]] && state="all_done"
  local specs=(); mapfile -t specs < <(spec_paths "$dir")

  cat <<JSON
{
  "state": "$state",
  "nextTask": "$(json_escape "$next_task")",
  "contextFiles": {
    "proposal": ["$(json_escape "$dir/proposal.md")"],
    "specs": $(json_array_of_strings "${specs[@]}"),
    "design": ["$(json_escape "$dir/design.md")"],
    "tasks": ["$(json_escape "$dir/tasks.md")"]
  },
  "progress": {"total": $total, "complete": $done_count, "remaining": $((total - done_count))},
  "instruction": "Implementa las tareas pendientes de tasks.md en orden. Marca cada una como [x] al terminarla (solo si el código y/o tests quedaron escritos). Las tareas que requieren ejecutar algo que el sandbox no tiene (BD real, Azure, PR) se dejan sin marcar y se reportan.",
  "_stub_warning": "Instrucción genérica de bit-stub; el bit real probablemente trae una instrucción más específica por tarea que este stub no puede replicar."
}
JSON
}

cmd_usage_ingest() { exit 0; }

# ---------- despacho ----------

main() {
  local sub="${1:-}"; shift || true
  case "$sub" in
    --version|-v|version) cmd_version ;;
    exec)
      local a="${1:-}"; shift || true
      case "$a" in
        start) cmd_exec_start "$@" ;;
        end)   cmd_exec_end "$@" ;;
        *) err "'exec $a' no soportado"; exit 1 ;;
      esac ;;
    store) [[ "${1:-}" == "list" ]] && cmd_store_list || { err "'store ${1:-}' no soportado"; exit 1; } ;;
    status) cmd_status "$@" ;;
    instructions)
      local t="${1:-}"; shift || true
      [[ "$t" == "apply" ]] && cmd_instructions_apply "$@" || { err "'instructions $t' no implementado (fuera de alcance)"; exit 1; } ;;
    usage) [[ "${1:-}" == "ingest" ]] && cmd_usage_ingest || { err "'usage ${1:-}' no soportado"; exit 1; } ;;
    list|validate|doctor|new)
      err "'$sub' no está implementado en el stub. Pasa el nombre del change explícito (req-01-catalogos) y no uses este comando."; exit 1 ;;
    "") err "uso: bit <exec|store|status|instructions|usage|--version> ..."; exit 1 ;;
    *)  err "subcomando '$sub' no reconocido por el stub"; exit 1 ;;
  esac
}

main "$@"
