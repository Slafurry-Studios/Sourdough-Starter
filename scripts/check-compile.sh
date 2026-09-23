#!/bin/sh
# Shared compile check — used by .githooks/pre-push and documented in AGENTS.md.
# Skip: SKIP_UNITY_COMPILE=1 ./scripts/check-compile.sh
# Env: UNITY_BIN (default: $HOME/Unity/Hub/Editor/2022.3.62f3/Editor/Unity)
set -e

if [ "${SKIP_UNITY_COMPILE:-0}" = "1" ]; then
  echo "check-compile: skipped (SKIP_UNITY_COMPILE=1)"
  exit 0
fi

ROOT="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
UNITY_BIN="${UNITY_BIN:-$HOME/Unity/Hub/Editor/2022.3.62f3/Editor/Unity}"
LOG="${TMPDIR:-/tmp}/sourdough-compile-$$.log"

if [ ! -x "$UNITY_BIN" ]; then
  echo "check-compile: Unity not found at $UNITY_BIN" >&2
  echo "check-compile: set UNITY_BIN or skip with SKIP_UNITY_COMPILE=1" >&2
  exit 1
fi

echo "check-compile: running Unity batchmode compile..."
set +e
"$UNITY_BIN" \
  -batchmode -nographics -quit \
  -projectPath "$ROOT" \
  -logFile "$LOG"
STATUS=$?
set -e

FAIL=0

if grep -n "error CS" "$LOG" 2>/dev/null; then
  echo ""
  echo "check-compile: COMPILE ERRORS:"
  grep -n "error CS" "$LOG" | tail -100
  FAIL=1
fi

if [ "$STATUS" -ne 0 ] || [ "$FAIL" -eq 1 ] || ! grep -q "Exiting batchmode successfully" "$LOG" 2>/dev/null; then
  echo ""
  echo "check-compile: FAILED (unity exit=$STATUS). Log: $LOG"
  exit 1
fi

echo "check-compile: OK"
rm -f "$LOG"
exit 0
