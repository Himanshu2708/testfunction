using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

#! Consider adding this function to a utility module. It also exists in AzureAdvisor
function Get-Secret {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $KeyVault,

        [Parameter(Mandatory = $true)]
        [string]
        $Secret
    )

    try {
        $tokenAuthURI = $Env:MSI_ENDPOINT + "?resource=https://vault.azure.net&api-version=2017-09-01"
        $tokenResponse = Invoke-RestMethod -Method Get -Headers @{"Secret" = "$env:MSI_SECRET" } -Uri $tokenAuthURI
        $accessToken = $tokenResponse.access_token

        # get secret value
        $authHeaders = @{"Authorization" = "bearer $accessToken" }
        $uri = "https://{0}.vault.azure.net/secrets/{1}?api-version=2016-10-01" -f $KeyVault, $Secret

        $secretWebResponse = Invoke-RestMethod -Method GET -Uri $uri -Headers $authHeaders
        $secretValue = $secretWebResponse.value
    
        return $secretValue
    }
    catch {
        Write-Host "Error 34"
        throw $_
    }
}


function Select-Scope {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        $scope,

        [parameter(Mandatory = $true)]
        $subscription
    )

    try {
        if ($scope -like "/providers/Microsoft.Management/managementGroups/*") {
            $result = $scope.replace("/providers/Microsoft.Management/managementGroups/", "Management Group: ")
        }
        elseif ($scope -like "/subscriptions/*") {
            $result = "Subscripcion: $subscription"
        }
        elseif ($scope -like "/") {
            $result = $scope = "Root Management Group" 
        }
        else {
            $result = $scope
        }
    }
    catch {
        # If something fails, returning the parameter scope unmodified.
        $result = $scope
    }

    return $result
}

function Get-ResourceGroupsPermissions {
    try {
        $sqlConnectionString = Get-Secret -Keyvault $ENV:KEYVAULT -Secret SQLConnectionString

        $date = Get-Date -Format yyyy-MM-dd
        $subscriptions = Get-AzSubscription

        $i = 0
        $allSubscriptions = $subscriptions.count

        foreach ($subscription in $subscriptions) {
            Set-AzContext -SubscriptionId $subscription.Id

            $i++
            Write-Host "($i of $allSubscriptions) - Exporting Resource Groups RBAC Permissions for subscription '$($subscription.Name)'"

            $resourceGroups = Get-AzResourceGroup
            $g = 0
            $gtotal = $resourceGroups.count

            foreach ($resourceGroup in $resourceGroups) {
                $g++
                Write-Host "($g of $gtotal) - Exporting RBAC Permissions for Resource Group '$($resourceGroup.ResourceGroupName)'"
                $assetResourceGroupName = $resourceGroup.ResourceGroupName
                $subscriptionId = $subscription.Id
                $assetSubscription = ($subscription.Name -replace "'", "''")
                $assetType = "ResourceGroup"

                $permissions = Get-AzRoleAssignment -ResourceGroupName $ResourceGroup

                foreach ($permission in $permissions) {
                    if (($permission.Scope -like "/") -or
                        ($permission.Scope -contains "/subscriptions/$subscriptionID") -or
                        ($permission.Scope -like "/providers/Microsoft.Management/managementGroups*") -or
                        ($permission.Scope -contains "/subscriptions/$subscriptionId/resourceGroups/$assetResourceGroupName")) {

                        $displayName = $permission.DisplayName
                        $login = $permission.SignInName
                        $role = $permission.RoleDefinitionName
                        $scope = Select-Scope -Scope $Permission.Scope -Subscription $subscription.Name
                        $userType = $permission.ObjectType
                        $query="INSERT INTO [dbo].[TEMP_RBAC]
                                ([date]
                                ,[subscription]
                                ,[resourcegroup]
                                ,[permissiontype]
                                ,[username]
                                ,[serviceprincipal]
                                ,[role]
                                ,[usertype]
                                ,[scope])
                            VALUES
                                ( '$date','$assetSubscription','$assetResourceGroupName','$assetType','$displayName','$login','$role','$userType','$scope')"

                        try {
                            Invoke-SqlCmd -ConnectionString $SQLConnectionString -Query $query -ErrorAction stop -WarningAction stop
                        }
                        catch {
                            Write-Host "Error executing INSERT Query: '$($query)'"
                            $_.Exception.Message
                        }
                    }
            
                }
                # After creating the temp table, the import stored procedure is run
                $query="EXEC sp_import_cloud_azure_rbac_permissions"
                try {
                    Invoke-SqlCmd -ConnectionString $SQLConnectionString -Query $query -ErrorAction stop -WarningAction stop
                }
                catch {
                    Write-Host "Error executing Query: '$($query)'"
                    $_.Exception.Message
                }
            }
        }
    }
    catch {
        write-host "Error 1356"
        throw $_
    }
}

function Get-SubscriptionsPermissions {
    try {
        $sqlConnectionString = Get-Secret -Keyvault $ENV:KEYVAULT -Secret SQLConnectionString
        
        $date = Get-Date -Format yyyy-MM-dd
        $subscriptions = Get-AzSubscription
        $i = 0

        #! Having a dejavu. We did this already
        $allSubscriptions = $subscriptions.count
        foreach ($subscription in $subscriptions) {
            Set-AzContext -Subscriptionid $subscription.Id

            $i++
            Write-Host "$i/$allSubscriptions - Exporting root RBAC permissions for subscription '$($subscription.Name)'"
            $subscriptionId = $subscription.Id
            $assetSubscription = ($subscription.Name -replace "'", "''")
            $assetType = "Subscription"
            $assetResourceGroupName = ""
            $permissions = Get-AzRoleAssignment
            foreach ($permission in $permissions) {
                if (($permission.Scope -like "/") -or
                    ($permission.Scope -contains "/subscriptions/$subscriptionId") -or
                    ($permission.Scope -like "/providers/Microsoft.Management/managementGroups*")) {

                    $displayName = $permission.DisplayName
                    $login = $permission.SignInName
                    $role = $permission.RoleDefinitionName
                    $scope = Select-Scope -Scope $permission.Scope -Subscription $subscription.Name
                    $userType = $permission.ObjectType                    
                    $query="INSERT INTO [dbo].[TEMP_RBAC]
                    ([date]
                    ,[subscription]
                    ,[resourcegroup]
                    ,[permissiontype]
                    ,[username]
                    ,[serviceprincipal]
                    ,[role]
                    ,[usertype]
                    ,[scope])
              VALUES
                    ( '$date','$assetSubscription','$assetResourceGroupName','$assetType','$displayName','$login','$role','$userType','$scope')"
                    try {
                        Invoke-SqlCmd -ConnectionString $SQLConnectionString -Query $query -ErrorAction stop -WarningAction stop
                    }
                    catch {
                        Write-Host "Error executing INSERT Query:  $query"
                        $_.Exception.Message
                    }
                }
<#                else {
                    #! If we're not going to use it just remove
                    #write-host $Permission.Scope
                }#>
            }
        }
    }
    catch {
        #! Is there a reason for the throw?
        throw $_.Exception.Message
    }
}

function Get-PermissionReport {
    try {
        $tenantId = Get-Secret -Keyvault $ENV:KEYVAULT -Secret TenantID
        $appId = Get-Secret -Keyvault $ENV:KEYVAULT -Secret ApplicationID
        $appSecret = Get-Secret -Keyvault $ENV:KEYVAULT -Secret ApplicationSecret
        $credential = New-Object System.Management.Automation.PSCredential($appId, (ConvertTo-SecureString $appSecret -AsPlainText -Force))
        
        $filename= "RBACpermissions-$date.csv"
        $path="c:\home\data\"


        
        Connect-AzAccount -Credential $credential -Tenant $tenantId -ServicePrincipal

        #Initializing File
        $date = Get-Date -Format yyyy-MM-dd
        ##"date,assetSubscription,assetResourceGroupName,assetType,displayName,login,role,userType,scope" |Out-File $paht$filename -Encoding unicode

        Get-ResourceGroupsPermissions

        Write-Host "`n"

        Get-SubscriptionsPermissions
        
       
    }
    catch {
        throw $_.Exception.Message
    }
}

#region Main

Set-Item Env:\SuppressAzurePowerShellBreakingChangeWarnings "true" ## We don't want anoying messages!!

$startDate = Get-Date
Write-Host "Process startTime : $startDate"


Get-PermissionReport
Write-Host "`n"

$endDate = Get-Date
Write-Host "Process endTime: $endDate"

$totalTime = $EndDate - $StartDate
Write-Host "Time consummed in minutes: $($totalTime.TotalMinutes)"
#endregion
