# Azure Infrastructure Deployment Script
# Prerequisite: Azure CLI (az) must be installed.

# 1. Ask for Azure Location
Write-Host "Allowed regions for your subscription: germanywestcentral, uaenorth, polandcentral, switzerlandnorth, swedencentral." -ForegroundColor Yellow
$location = Read-Host -Prompt "Enter Azure Region [Default: swedencentral]"
if ([string]::IsNullOrWhiteSpace($location)) {
    $location = "swedencentral"
}

$resourceGroupName = "diceus-claims-$location-rg"
$sqlAdminUser = "sqladmin"

# 2. Ask for secure SQL Administrator Password
$securePassword = Read-Host -Prompt "Enter administrator password for Azure SQL Server (Min 8 chars, must contain uppercase, lowercase, number, symbol)"
if ($securePassword.Length -lt 8) {
    Write-Error "Password is too short. Deployment aborted."
    exit
}

# 3. Check if logged into Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show --query name -o tsv 2>$null
if ($null -eq $account) {
    Write-Host "You are not logged in. Redirecting to Azure login page..." -ForegroundColor Cyan
    az login
} else {
    Write-Host "Logged into subscription: $account" -ForegroundColor Green
}

# 4. Create Azure Resource Group
Write-Host "Creating Resource Group '$resourceGroupName' in '$location'..." -ForegroundColor Yellow
az group create --name $resourceGroupName --location $location

# 5. Deploy Bicep Template
Write-Host "Deploying Bicep template (this may take 2-4 minutes)..." -ForegroundColor Yellow
$deployment = az deployment group create `
    --resource-group $resourceGroupName `
    --template-file "./main.bicep" `
    --parameters sqlAdminUsername=$sqlAdminUser sqlAdminPassword=$securePassword location=$location `
    --query "properties.outputs" -o json | ConvertFrom-Json

# 6. Extract outputs and download Publish Profiles
if ($deployment) {
    $backendUrl = $deployment.backendUrl.value
    $frontendUrl = $deployment.frontendUrl.value
    $sqlServerFqdn = $deployment.sqlServerFqdn.value
    $backendAppName = $deployment.backendAppName.value
    $frontendAppName = $deployment.frontendAppName.value

    Write-Host "`n==========================================" -ForegroundColor Green
    Write-Host "        Deployment Completed!             " -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "Backend API URL:       $backendUrl" -ForegroundColor Cyan
    Write-Host "Frontend App URL:      $frontendUrl" -ForegroundColor Cyan
    Write-Host "SQL Server Hostname:   $sqlServerFqdn" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Green

    Write-Host "`nDownloading publish profiles..." -ForegroundColor Yellow
    
    $backendPublishProfile = az webapp deployment list-publishing-profiles --name $backendAppName --resource-group $resourceGroupName --xml
    $frontendPublishProfile = az webapp deployment list-publishing-profiles --name $frontendAppName --resource-group $resourceGroupName --xml

    $backendProfilePath = Join-Path $PSScriptRoot "backend-publish-profile.xml"
    $frontendProfilePath = Join-Path $PSScriptRoot "frontend-publish-profile.xml"

    $backendPublishProfile | Out-File -FilePath $backendProfilePath -Encoding utf8
    $frontendPublishProfile | Out-File -FilePath $frontendProfilePath -Encoding utf8

    Write-Host "Backend Publish Profile saved to:  $backendProfilePath" -ForegroundColor Green
    Write-Host "Frontend Publish Profile saved to: $frontendProfilePath" -ForegroundColor Green

    Write-Host "`n==========================================" -ForegroundColor Green
    Write-Host "IMPORTANT FOR CI/CD GITHUB ACTIONS CONFIGURATION:" -ForegroundColor Yellow
    Write-Host "Please register the following Secrets in your GitHub repository settings:" -ForegroundColor Yellow
    Write-Host "1. AZURE_WEBAPP_PUBLISH_PROFILE     -> Copy contents of backend-publish-profile.xml" -ForegroundColor Cyan
    Write-Host "2. AZURE_FRONTEND_PUBLISH_PROFILE   -> Copy contents of frontend-publish-profile.xml" -ForegroundColor Cyan
    Write-Host "3. AZURE_BACKEND_URL                -> (value: $backendUrl)" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Green
} else {
    Write-Error "Deployment failed."
}
