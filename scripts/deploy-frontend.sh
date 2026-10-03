#!/usr/bin/env bash
# Publica o front-end no contêiner $web (Azure Blob Storage static website).
set -euo pipefail
cd "$(dirname "$0")/.."
source .env.local

rm -rf publish/web && mkdir -p publish/web
cp -R frontend/. publish/web/
echo "window.TECHSTORE_CONFIG = { apiBaseUrl: \"$API_URL\" };" > publish/web/config.js

az storage blob upload-batch --account-name "$STORAGE_ACCOUNT" --auth-mode key \
  --destination '$web' --source publish/web --overwrite -o none

echo "Front-end publicado: $FRONTEND_URL"
