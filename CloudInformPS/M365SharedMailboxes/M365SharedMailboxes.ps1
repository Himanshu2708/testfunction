using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

function Get-Secret {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $keyVault,
        [Parameter(Mandatory = $true)]
        [string]
        $Secret
    )
    try {
        # get access token with MSI. For more details, please refer to https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity#rest-protocol-examples
        $tokenAuthURI = $Env:MSI_ENDPOINT + "?resource=https://vault.azure.net&api-version=2017-09-01"
        $tokenResponse = Invoke-RestMethod -Method Get -Headers @{"Secret" = "$env:MSI_SECRET" } -Uri $tokenAuthURI
        $accessToken = $tokenResponse.access_token
        # get secret value
        $authHeaders = @{"Authorization" = "bearer $accessToken" }
        $uri = "https://" + $keyVault + ".vault.azure.net/secrets/" + $Secret + "?api-version=2016-10-01"
        $SecretWebResponse = Invoke-RestMethod -Method GET -Uri $uri -Headers $authHeaders
        $SecretValue = $SecretWebResponse.value
        #write-host "SECRET : $secretValue"
        return $secretValue
    }
    catch {
        throw $_
    }
}

function Connect-ExoPowershell {
    try {

        $clientID = Get-Secret -keyVault $ENV:KEYVAULT -Secret 'ApplicationID'
        $clientSecret = Get-Secret -keyVault $ENV:KEYVAULT -Secret 'ApplicationSecret'
        $tenantId = Get-Secret -keyVault $ENV:KEYVAULT -Secret 'TenantID'
        write-host "Connecting to Powershell"
        $body = @{client_id = $clientID; client_secret = $clientSecret; grant_type = "client_credentials"; scope = "https://outlook.office365.com/.default"; }
        $oAuthReq = Invoke-RestMethod -Method Post -Uri https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token -Body $body
        $tokenType = $oAuthReq.token_type
        $accessToken = $oAuthReq.access_token

        $authorization = "Bearer {0}" -f $accessToken
        $password = ConvertTo-SecureString -AsPlainText $authorization -Force
        $ctoken = New-Object System.Management.Automation.PSCredential -ArgumentList "OAuthUser@$tenantId", $password

        $session = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri https://outlook.office365.com/PowerShell-LiveId?BasicAuthToOAuthConversion=true -Credential $Ctoken -Authentication Basic -AllowRedirection  -Verbose
        write-host "Connected to Exchange Powershell"
        Import-PSSession $session
    }
    catch {
        Throw $_
    }

}

Function Get-Mailboxes {
    Try {
        write-host "Recovering Mailboxes"
        $data = Get-Mailbox | Select-Object DisplayName, WindowsEmailAddress, RetentionHoldEnabled, LitigationHoldEnabled, RecipientTypeDetails
        $ouput = $data | convertto-json
        Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
            StatusCode = [HttpStatusCode]::OK
            Headers = @{
                "Content-type" = "application/json"
            }
            Body = [Ordered]@{
                "data" = $data
                "message" = "Mailboxes data recovered successfully" 
            } | ConvertTo-Json
        })
    }
    
    Catch {
        Write-Host "An error occurred:"
        Write-Host $_.ScriptStackTrace
        Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
            StatusCode = [HttpStatusCode]::InternalServerError
            Headers = @{
                "Content-type" = "application/json"
            }
            Body = [Ordered]@{
                "stack_trace" = $_.ScriptStackTrace
                "message" = "An error occurred." 
            } | ConvertTo-Json
        })
    }
}

Function Main {
    try {
        $a = Get-PSSession
        foreach ($b in $a) { Remove-PSSession $b.Id }
        Connect-ExoPowershell

    ## Get M365 Mailboxes properties    
        Get-Mailboxes
    }
    catch {
        throw $_
    }
    finally {
        $session=Get-PSSession
        remove-pssession $session
    }

}

Main