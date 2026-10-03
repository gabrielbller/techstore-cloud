#!/usr/bin/env bash
# Cria toda a infraestrutura do MVP na Azure (Bicep), habilita o static website
# e grava no GitHub as variáveis usadas pelo pipeline. Pré-requisitos: az login e gh auth login.
set -euo pipefail
cd "$(dirname "$0")/.."

RESOURCE_GROUP="${RESOURCE_GROUP:-rg-techstore}"
LOCATION="${LOCATION:-mexicocentral}"  # Azure for Students não permite Brazil South
GITHUB_REPO="${GITHUB_REPO:-gabrielbller/techstore-cloud}"

# A senha do SQL é gerada uma vez e fica em .env.local (fora do Git) e no Key Vault.
touch .env.local && chmod 600 .env.local
source .env.local
if [ -z "${SQL_ADMIN_PASSWORD:-}" ]; then
  SQL_ADMIN_PASSWORD="Ts$(openssl rand -hex 12)!"
  echo "SQL_ADMIN_PASSWORD='$SQL_ADMIN_PASSWORD'" >> .env.local
fi

echo "==> Resource group $RESOURCE_GROUP ($LOCATION)"
az group create -n "$RESOURCE_GROUP" -l "$LOCATION" --tags projeto=techstore-cloud ambiente=mvp -o none

# O GitHub identifica o repositório por IDs imutáveis no token OIDC.
GITHUB_SUBJECT="$(gh api "repos/$GITHUB_REPO" --jq '"repo:\(.owner.login)@\(.owner.id)/\(.name)@\(.id):ref:refs/heads/main"')"

echo "==> Implantando infra/main.bicep"
az deployment group create -g "$RESOURCE_GROUP" -n techstore -f infra/main.bicep \
  -p adminIp="$(curl -s https://api.ipify.org)" sqlAdminPassword="$SQL_ADMIN_PASSWORD" githubSubject="$GITHUB_SUBJECT" \
     alertEmail="$(az account show --query user.name -o tsv)" \
  --query properties.outputs -o json > .saidas.json

saida() { python3 -c "import json; print(json.load(open('.saidas.json'))['$1']['value'])"; }

echo "==> Habilitando static website"
az storage blob service-properties update --account-name "$(saida storageAccount)" --auth-mode key \
  --static-website --index-document index.html --404-document 404.html -o none

echo "==> Variáveis do pipeline no GitHub ($GITHUB_REPO)"
gh variable set AZURE_CLIENT_ID --repo "$GITHUB_REPO" --body "$(saida githubClientId)"
gh variable set AZURE_TENANT_ID --repo "$GITHUB_REPO" --body "$(az account show --query tenantId -o tsv)"
gh variable set AZURE_SUBSCRIPTION_ID --repo "$GITHUB_REPO" --body "$(az account show --query id -o tsv)"
gh variable set AZURE_WEBAPP_NAME --repo "$GITHUB_REPO" --body "$(saida apiName)"
gh variable set AZURE_STORAGE_ACCOUNT --repo "$GITHUB_REPO" --body "$(saida storageAccount)"
gh variable set API_URL --repo "$GITHUB_REPO" --body "$(saida apiUrl)"

cat >> .env.local <<EOF
RESOURCE_GROUP=$RESOURCE_GROUP
API_NAME=$(saida apiName)
STORAGE_ACCOUNT=$(saida storageAccount)
API_URL=$(saida apiUrl)
FRONTEND_URL=$(saida frontendUrl)
EOF

echo
echo "Front-end: $(saida frontendUrl)"
echo "API:       $(saida apiUrl)"
