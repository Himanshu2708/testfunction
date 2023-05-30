# Input bindings are passed in via param block.
param($Timer)

#! Function App name is hardcoded. Use $ENV:WEBSITE_NAME

$paramWebRequest = @{
    Uri    = "https://$($ENV:WEBSITE_NAME).azurewebsites.net/api/PermissionReport"
    Method = 'GET'
}

Invoke-WebRequest @paramWebRequest
