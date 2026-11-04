#!/usr/bin/env bash
# Builds the frontend, copies it into the API's wwwroot, and starts FlipFlow on one port.
#   ./run.sh                    build everything and start on http://localhost:5000
#   ./run.sh --skip-frontend    reuse the frontend already in wwwroot
# Optional environment variables:
#   ASPNETCORE_URLS         e.g. http://0.0.0.0:5000 to allow other devices on your network
#   Access__Passcode        set to require a passcode (recommended if you bind to 0.0.0.0)
#   DemoData__Enabled=true  add a couple of sample items to an empty database
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WWWROOT="$ROOT/src/backend/FlipFlow.Api/wwwroot"

if [[ "${1:-}" != "--skip-frontend" ]]; then
  echo "==> Building frontend"
  cd "$ROOT/src/frontend"
  [[ -d node_modules ]] || npm ci
  npm run build

  echo "==> Copying frontend into wwwroot"
  find "$WWWROOT" -mindepth 1 -not -name '.gitkeep' -delete
  cp -R dist/. "$WWWROOT/"
fi

echo "==> Starting FlipFlow (Ctrl+C to stop)"
cd "$ROOT/src/backend/FlipFlow.Api"
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Production}"
exec dotnet run -c Release --no-launch-profile
