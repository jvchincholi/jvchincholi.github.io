param location string = resourceGroup().location
param appName string = 'my-dotnet10-api'

// 1. The Environment (The "Server Cluster")
resource env 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${appName}-env'
  location: location
  properties: {
    appLogsConfiguration: { destination: 'log-analytics' }
  }
}

// 2. The Container App (Your API)
resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  location: location
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      ingress: {
        external: true // This creates the public URL
        targetPort: 8080
      }
    }
    template: {
      containers: [{
          image: 'mcr.microsoft.com/dotnet/samples:aspnetapp' // Placeholder image
          name: appName
          env: [
                  name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
                  value: appInsightsConnectionString // This variable comes from your App Insights resource
          ]
      }]
    }
  }
}

// 3. OUTPUT: This is where your URL comes from!
output url string = 'https://${app.properties.configuration.ingress.fqdn}'