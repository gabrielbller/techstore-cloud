// TechStore Cloud - infraestrutura do MVP de cadastro de produtos.
// Uso: ./scripts/deploy-infra.sh

@description('Região dos recursos (padrão: a do resource group).')
param location string = resourceGroup().location

@description('IP público do administrador, liberado no firewall do SQL (Query Editor).')
param adminIp string

@description('Login do administrador do Azure SQL.')
param sqlAdminLogin string = 'techstoreadmin'

@secure()
@description('Senha do administrador do Azure SQL (fica apenas no Key Vault).')
param sqlAdminPassword string

@description('Plano do App Service: F1 (gratuito) ou B1 (sempre ligado, pago).')
@allowed([ 'B1', 'F1' ])
param appServiceSku string = 'F1'

@description('Versão do .NET no App Service.')
param dotnetVersion string = '10.0'

@description('Repositório GitHub autorizado a fazer deploy (OIDC).')
param githubRepo string = 'gabrielbller/techstore-cloud'

var sufixo = substring(uniqueString(resourceGroup().id), 0, 6)
var tags = {
  projeto: 'techstore-cloud'
  ambiente: 'mvp'
}

// ---------------------------------------------------------------- Monitoramento
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-techstore'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-techstore'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// ---------------------------------------------------------------- Front-end (static website)
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'sttechstore${sufixo}'
  location: location
  tags: tags
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

// ---------------------------------------------------------------- Banco de dados
resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: 'sql-techstore-${sufixo}'
  location: location
  tags: tags
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sqlServer
  name: 'sqldb-produtos'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    requestedBackupStorageRedundancy: 'Local' // backups ficam no Brasil
  }
}

// Libera serviços da Azure (App Service) e o IP do administrador.
resource sqlFirewallAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlFirewallAdmin 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'administrador'
  properties: {
    startIpAddress: adminIp
    endIpAddress: adminIp
  }
}

// ---------------------------------------------------------------- Segredos
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-techstore-${sufixo}'
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: { family: 'A', name: 'standard' }
    enableRbacAuthorization: true
    softDeleteRetentionInDays: 7
  }
}

resource segredoConexao 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'sql-connection-string'
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDatabase.name};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;'
  }
}

// ---------------------------------------------------------------- API (App Service)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-techstore'
  location: location
  tags: tags
  kind: 'linux'
  sku: { name: appServiceSku }
  properties: { reserved: true }
}

resource api 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-techstore-api-${sufixo}'
  location: location
  tags: tags
  kind: 'app,linux'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|${dotnetVersion}'
      alwaysOn: appServiceSku != 'F1'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      appSettings: [
        {
          // Key Vault reference: o App Service lê o segredo com a Managed Identity.
          name: 'ConnectionStrings__ProdutosDb'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${segredoConexao.name})'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'Cors__AllowedOrigins__0'
          value: replace(storage.properties.primaryEndpoints.web, '.net/', '.net')
        }
      ]
    }
  }
}

// A identidade da API só pode LER segredos (menor privilégio).
resource apiLeSegredos 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, api.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: api.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Logs HTTP, de console e de plataforma do App Service no Log Analytics.
resource apiDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'log-analytics'
  scope: api
  properties: {
    workspaceId: logAnalytics.id
    logs: [ { categoryGroup: 'allLogs', enabled: true } ]
    metrics: [ { category: 'AllMetrics', enabled: true } ]
  }
}

// ---------------------------------------------------------------- CI/CD (GitHub Actions via OIDC, sem senha)
resource githubIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-techstore-github'
  location: location
  tags: tags
}

resource githubCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: githubIdentity
  name: 'github-main'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: 'repo:${githubRepo}:ref:refs/heads/main'
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

// O pipeline pode publicar a API (Website Contributor) e o front-end (Storage Blob Data Contributor).
resource githubPublicaApi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(api.id, githubIdentity.id, 'Website Contributor')
  scope: api
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772')
    principalId: githubIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource githubPublicaSite 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, githubIdentity.id, 'Storage Blob Data Contributor')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: githubIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------------- Saídas
output frontendUrl string = storage.properties.primaryEndpoints.web
output apiUrl string = 'https://${api.properties.defaultHostName}'
output apiName string = api.name
output storageAccount string = storage.name
output sqlServer string = sqlServer.properties.fullyQualifiedDomainName
output keyVault string = keyVault.name
output githubClientId string = githubIdentity.properties.clientId
