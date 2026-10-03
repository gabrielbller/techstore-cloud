#!/usr/bin/env bash
# Deploy manual da API no App Service (o pipeline faz o mesmo a cada push na main).
set -euo pipefail
cd "$(dirname "$0")/.."
source .env.local

rm -rf publish/api publish/api.zip
dotnet publish src/TechStore.Produtos.Api -c Release -o publish/api
(cd publish/api && zip -qr ../api.zip .)

az webapp deploy -g "$RESOURCE_GROUP" -n "$API_NAME" --src-path publish/api.zip --type zip -o none
curl -sf --retry 10 --retry-delay 10 --retry-all-errors "$API_URL/health" && echo " <- API no ar: $API_URL"
