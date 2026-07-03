@description('The location where resources will be deployed.')
param location string = resourceGroup().location

@description('Prefix for resource names to guarantee uniqueness.')
param prefix string = 'diceus-claims-${uniqueString(resourceGroup().id)}'

@description('The administrator username for the Azure SQL Database.')
param sqlAdminUsername string = 'sqladmin'

@description('The administrator password for the Azure SQL Database.')
@secure()
param sqlAdminPassword string

// 1. Storage Account (for Claim Documents)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: take(replace(toLower('diceus${uniqueString(resourceGroup().id)}'), '-', ''), 24)
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
  }
}

// Create blob services container
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/claim-documents'
}

// 2. Azure SQL Database
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: '${prefix}-sqlserver'
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

// Configure firewall to allow Azure Services (App Service, GitHub Actions runner) to connect to database
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Create SQL Database in Free / Basic tier (10 DTUs, 2GB size)
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'ClaimsModuleDb'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    maxSizeBytes: 2147483648 // 2 GB
  }
}

// 3. App Service (Backend API)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${prefix}-asp'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux container hosting
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${prefix}-api'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'ConnectionStrings__AzureBlobStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'StorageProvider'
          value: 'AzureBlob'
        }
        {
          name: 'Jwt__Key'
          value: 'DiceusClaimsManagementSystemSecretKey2026'
        }
        {
          name: 'Jwt__Issuer'
          value: 'DiceusClaimsAPI'
        }
        {
          name: 'Jwt__Audience'
          value: 'DiceusClaimsApp'
        }
      ]
    }
  }
}

// 4. Azure Web App (Frontend Angular served via Node.js server)
resource frontendWebApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${prefix}-frontend'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'NODE|20-lts'
      appCommandLine: 'node server.js'
    }
  }
}

output backendUrl string = 'https://${webApp.properties.defaultHostName}'
output frontendUrl string = 'https://${frontendWebApp.properties.defaultHostName}'
output backendAppName string = webApp.name
output frontendAppName string = frontendWebApp.name
output sqlServerFqdn string = '${sqlServer.name}.database.windows.net'
