<#
.SYNOPSIS
Provision an Azure SQL Database.

.DESCRIPTION
This script uses the provided subscription to create a SQL Server, it then
downloads a copy of the AdventureWorks datbase, and uploads it to the server.

.PARAMETER SubscriptionName
The name of the Azure subscription to use.

.PARAMETER Location
The location of the SQL Server (e.g. West US).

.PARAMETER SqlServerAdminLogin
The administrative login name to use for the SQL Server.

.PARAMETER SqlServerAdminPassword
The password for the administrative login of the  SQL Server.
#>
Param
(
	[Parameter (Mandatory = $true)]
	[string] $SubscriptionName,

    [Parameter (Mandatory = $true)]
    [String] $Location,

    [Parameter (Mandatory = $true)]
    [String] $SqlServerAdminLogin,

    [Parameter (Mandatory = $true)]
    [String] $SqlServerAdminPassword,

    [String] $FirewallRuleName = "DeleteThisRule"
)

# Make the script stop on error
$ErrorActionPreference = "Stop"

# The human friendly link for downloading the AdventureWorks database is
# http://msftdbprodsamples.codeplex.com/releases/view/37304
# However, this does a client-side redirect and ultimately resolves to:
$downloadUrl = "http://download-codeplex.sec.s-msft.com/Download/Release?ProjectName=msftdbprodsamples&DownloadId=342357&FileTime=129994018385870000&Build=20959" 

# What will we name the downloaded zip file?
$downloadedZipFile = "database.zip"

# Where will we expand the zip file to?
$tempFolderPath = ".\temp"

# Check the azure module is installed
if(-not(Get-Module -name "Azure")) 
{ 
    if(Get-Module -ListAvailable | Where-Object { $_.name -eq "Azure" }) 
    { 
        Import-Module Azure
    }
    else
    {
        "Microsoft Azure Powershell has not been installed, or cannot be found."
        Exit
    }
}

# Login to the given subscription
Add-AzureAccount
Select-AzureSubscription -SubscriptionName $SubscriptionName

"Creating server"
$serverContext = New-AzureSqlDatabaseServer -location $Location -AdministratorLogin $SqlServerAdminLogin -AdministratorLoginPassword $SqlServerAdminPassword
$serverName = $serverContext.ServerName
"Created server: " + $serverName

# $localIPAddress = (gwmi Win32_NetworkAdapterConfiguration | ? { $_.IPAddress -ne $null }).ipaddress.Split()[0]

"Creating a dangerous firewall rule"
$ruleContext = New-AzureSqlDatabaseServerFirewallRule -ServerName $serverName -RuleName $FirewallRuleName -StartIPAddress "0.0.0.0" -EndIPAddress "255.255.255.255"

# Download the AdventureWorks database
if(!(Split-Path -parent $downloadedZipFile) -or !(Test-Path -pathType Container (Split-Path -parent $downloadedZipFile))) { 
    $downloadedZipFile = Join-Path $pwd (Split-Path -leaf $downloadedZipFile) 
} 
      
"Downloading [$downloadUrl]`nSaving at [$downloadedZipFile]" 
$client = new-object System.Net.WebClient 
$client.DownloadFile($downloadUrl, $downloadedZipFile) 
"Download complete"

$unzipped = New-Item -Path $tempFolderPath -ItemType directory
"Unzipping to:"
$unzipped.FullName

$helper = New-Object -ComObject Shell.Application
$files = $helper.NameSpace($downloadedZipFile).Items()
$helper.NameSpace($unzipped.FullName).CopyHere($files)

"Running CreateAdventureWorksForSQLAzure.cmd to import the data to SQL Azure"
 
$qualifiedServerName = $serverName + ".database.windows.net"

$originalDirectory = $pwd
# We expect the unzipped file to have a specific file/folder structure
# and we need to run the cmd from the folder where it resides.
cd "$tempFolderPath\AdventureWorks"
.\CreateAdventureWorksForSQLAzure.cmd $qualifiedServerName $SqlServerAdminLogin $SqlServerAdminPassword
cd $originalDirectory

# TODO: delete the temp folder?
# TODO: delete the downloaded zip?

"The AdventureWorks database has been uploaded to your Azure subscription."
"Make a note of the server name:"
Write-Host -BackgroundColor Black -ForegroundColor DarkYellow -Object $serverName
Write-Host -BackgroundColor Black -ForegroundColor Red " * WARNING * "
"The firewall rule named '$FirewallRuleName' allows any IP address to access the server."
"The rule should be removed or modified as soon as your test is complete."
