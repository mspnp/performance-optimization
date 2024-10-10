@description('Location for all resources.')
param location string = resourceGroup().location

@description('The administrator username of the SQL logical server.')
param administratorLogin string = 'myadminname'

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string = newGuid()

@description('The Microsoft Entra ID user to be database admin')
param user string

@description('The object id of the previous user')
param userObjectId string

@description('The tenant id of the previous user')
param userTenantId string

// --- Variables
var uniqueName = uniqueString(resourceGroup().id)

@description('The name of the SQL logical server.')
var serverName = 'sqlserver-${uniqueName}'
@description('The name of the SQL Database.')
var sqlDBName = 'busyDatabase-${uniqueName}'
var logAnalyticsWorkspaceName = 'busyDatabase-${uniqueName}'


resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    minimalTlsVersion: '1.2'
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }
  resource allowAzureServicesRule 'firewallRules' = {
    name: 'AllowAllWindowsAzureIps'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
  resource activeDirectoryAdmin 'administrators@2023-08-01-preview' = {
    name: 'ActiveDirectory'
    properties: {
      administratorType: 'ActiveDirectory'
      login: user
      sid: userObjectId
      tenantId: userTenantId
    }
  }
  resource sqlADOnlyAuth 'azureADOnlyAuthentications@2023-08-01-preview' = {
    name: 'Default'
    properties: {
      azureADOnlyAuthentication: true
    }
    dependsOn: [
      activeDirectoryAdmin
    ]
  }
}

resource diagnosticSettingsSqlServer 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: sqlServer
  name: '${sqlServer.name}-diag'
  properties: {
    workspaceId: logAnalyticsWorkspace.id
  }
}

resource auditingServerSettings 'Microsoft.Sql/servers/auditingSettings@2021-11-01-preview' = {
  parent: sqlServer
  name: 'default'
  properties: {
    state: 'Enabled'
    isAzureMonitorTargetEnabled: true
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
  }
}

resource sqlVulnerabilityAssessment 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2022-11-01-preview' = {
  name: 'default'
  parent: sqlServer
  properties: {
    state: 'Enabled'
  }
  dependsOn: [
    auditingServerSettings
  ]
}

resource sqlDB 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDBName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    sampleName: 'AdventureWorksLT'
  }
  dependsOn: [
    sqlServer::sqlADOnlyAuth
    sqlServer::activeDirectoryAdmin
    auditingServerSettings
    sqlVulnerabilityAssessment
  ]
}

resource auditingDbSettings 'Microsoft.Sql/servers/databases/auditingSettings@2023-08-01-preview' = {
  parent: sqlDB
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    state: 'Enabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018' // Example SKU, adjust as needed
    }
    retentionInDays: 30 // Adjust retention period as needed
  }
}

resource diagnosticSettingsSqlDb 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: sqlDB
  name: '${sqlDB.name}-diag'
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'SQLSecurityAuditEvents'
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
    ]
  }
}
