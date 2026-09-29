#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACTS="${ROOT}/artifacts/tool"
PACKAGE_ID="DotAi"
VERSION="${DOTAI_VERSION:-0.1.0}"

DOTNET="${DOTNET:-dotnet}"
if ! command -v "$DOTNET" >/dev/null 2>&1; then
  if [[ -x "${HOME}/.dotnet/dotnet" ]]; then
    DOTNET="${HOME}/.dotnet/dotnet"
  else
    echo "dotnet SDK not found. Install .NET 10 SDK: https://dotnet.microsoft.com/download" >&2
    exit 1
  fi
fi

mkdir -p "${ARTIFACTS}"

echo "Packing ${PACKAGE_ID} ${VERSION}..."
"$DOTNET" pack "${ROOT}/Presentation/Presentation.csproj" \
  -c Release \
  -o "${ARTIFACTS}" \
  -p:Version="${VERSION}"

echo "Installing global tool ${PACKAGE_ID}..."
"$DOTNET" tool uninstall -g "${PACKAGE_ID}" >/dev/null 2>&1 || true
"$DOTNET" tool install -g "${PACKAGE_ID}" \
  --add-source "${ARTIFACTS}" \
  --version "${VERSION}"

echo
echo "Installed. Ensure the .NET tools directory is on PATH:"
echo "  export PATH=\"\$PATH:\$HOME/.dotnet/tools\""
echo
if [[ -d "${HOME}/.dotnet" ]] && [[ ! -d /usr/share/dotnet ]]; then
  echo "If you installed the SDK under ~/.dotnet, also set:"
  echo "  export DOTNET_ROOT=\"\$HOME/.dotnet\""
  echo
fi
echo "Then from any git repo:"
echo "  dot-ai init"
echo "  dot-ai --help"
