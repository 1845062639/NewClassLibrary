#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BROKER_HOST="${BROKER_HOST:-127.0.0.1}"
BROKER_PORT="${BROKER_PORT:-1883}"
TOPIC_PREFIX="${TOPIC_PREFIX:-stnext}"
APP_CLIENT_ID="${APP_CLIENT_ID:-stnext-app-smoke}"
TEST_CLIENT_ID="${TEST_CLIENT_ID:-stnext-test-smoke}"
APP_LOG="${APP_LOG:-$ROOT/artifacts/logs/app-mqtt-smoke.log}"
TEST_LOG="${TEST_LOG:-$ROOT/artifacts/logs/test-mqtt-smoke.log}"
TEST_DB="${TEST_DB:-$ROOT/artifacts/test-persistence/mqtt-smoke.db}"
RUN_SECONDS="${RUN_SECONDS:-20}"

mkdir -p "$(dirname "$APP_LOG")" "$(dirname "$TEST_LOG")" "$(dirname "$TEST_DB")"
: > "$APP_LOG"
: > "$TEST_LOG"

cleanup() {
  local code=$?
  [[ -n "${APP_PID:-}" ]] && kill "$APP_PID" 2>/dev/null || true
  [[ -n "${TEST_PID:-}" ]] && kill "$TEST_PID" 2>/dev/null || true
  wait 2>/dev/null || true
  exit $code
}
trap cleanup EXIT INT TERM

cd "$ROOT"

echo "[mqtt-smoke] broker=${BROKER_HOST}:${BROKER_PORT} topicPrefix=${TOPIC_PREFIX} runSeconds=${RUN_SECONDS}"

dotnet build StandardTestNext.sln --no-restore >/dev/null

STNEXT_MESSAGE_BUS_HOST="$BROKER_HOST" \
STNEXT_MESSAGE_BUS_PORT="$BROKER_PORT" \
STNEXT_MESSAGE_BUS_TOPIC_PREFIX="$TOPIC_PREFIX" \
STNEXT_MESSAGE_BUS_CLIENT_ID="$APP_CLIENT_ID" \
nohup dotnet run --project StandardTestNext.App -- \
  --message-bus mqtt \
  --message-bus-host "$BROKER_HOST" \
  --message-bus-port "$BROKER_PORT" \
  --message-bus-topic-prefix "$TOPIC_PREFIX" \
  --message-bus-client-id "$APP_CLIENT_ID" \
  >"$APP_LOG" 2>&1 &
APP_PID=$!

echo "[mqtt-smoke] app pid=$APP_PID log=$APP_LOG"
sleep 3

STNEXT_MESSAGE_BUS_HOST="$BROKER_HOST" \
STNEXT_MESSAGE_BUS_PORT="$BROKER_PORT" \
STNEXT_MESSAGE_BUS_TOPIC_PREFIX="$TOPIC_PREFIX" \
STNEXT_MESSAGE_BUS_CLIENT_ID="$TEST_CLIENT_ID" \
nohup dotnet run --project StandardTestNext.Test -- \
  --message-bus mqtt \
  --message-bus-host "$BROKER_HOST" \
  --message-bus-port "$BROKER_PORT" \
  --message-bus-topic-prefix "$TOPIC_PREFIX" \
  --message-bus-client-id "$TEST_CLIENT_ID" \
  --persistence sqlite \
  --sqlite-db "$TEST_DB" \
  >"$TEST_LOG" 2>&1 &
TEST_PID=$!

echo "[mqtt-smoke] test pid=$TEST_PID log=$TEST_LOG"
sleep "$RUN_SECONDS"

kill "$APP_PID" "$TEST_PID" 2>/dev/null || true
wait "$APP_PID" "$TEST_PID" 2>/dev/null || true

echo "[mqtt-smoke] completed"
echo "--- app tail ---"
tail -n 20 "$APP_LOG" || true
echo "--- test tail ---"
tail -n 20 "$TEST_LOG" || true
