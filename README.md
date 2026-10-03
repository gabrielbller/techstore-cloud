# TechStore Cloud — Cadastro de Produtos na Azure

MVP do sistema de cadastro de produtos da TechStore Cloud, feito para a disciplina
*Sistemas em Nuvem com Azure*.

| Componente | Serviço Azure |
|---|---|
| Front-end estático (HTML/CSS/JS) | Azure Blob Storage — static website |
| API REST .NET 10 | Azure App Service (Linux) |
| Banco de dados | Azure SQL Database (Basic) |
| Segredo (connection string) | Azure Key Vault (Key Vault reference) + Managed Identity |
| Monitoramento | Azure Monitor, Log Analytics, Application Insights |
| Infraestrutura como código | Bicep (`infra/main.bicep`) |
| CI/CD | GitHub Actions com OIDC (build, testes, deploy da API e do front-end) |

## Estrutura

```
src/TechStore.Produtos.Api      API REST (Minimal API + EF Core)
tests/                          testes de integração (xUnit)
frontend/                       site estático
infra/main.bicep                todos os recursos Azure
scripts/                        deploy da infra e deploy manual
docs/consultas.kql              consultas do Log Analytics
```

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/produtos?busca=` | Lista produtos |
| GET | `/api/produtos/{id}` | Detalhe |
| POST | `/api/produtos` | Cadastra |
| PUT | `/api/produtos/{id}` | Atualiza |
| DELETE | `/api/produtos/{id}` | Exclui |
| GET | `/health` | Saúde da API e do banco |
| GET | `/scalar` | Documentação (OpenAPI) |

## Implantar na Azure

Pré-requisitos: Azure CLI, GitHub CLI, .NET 10 SDK. Depois do primeiro deploy, cada push na `main` publica API e front-end pelo GitHub Actions.

```bash
az login && gh auth login
./scripts/deploy-infra.sh      # cria todos os recursos (Bicep)
./scripts/deploy-api.sh        # publica a API no App Service
./scripts/deploy-frontend.sh   # publica o site no Blob Storage
```

Para remover tudo: `az group delete -n rg-techstore`.

## Rodar localmente

```bash
docker compose up -d
dotnet run --project src/TechStore.Produtos.Api --launch-profile http
python3 -m http.server 8080 --directory frontend
```
