using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudInform.AzureAPIWrapper.Models;

namespace CloudInform
{
    internal class M365Graph
    {
        public static async Task GetM365TeamsAudit(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365TeamsAudit report");

                int counter = 0;
                string startDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string tenantId = Common.GetSecret("TenantID", log).Value;
                string filePath = "C:/home/data/";
                string fileName = "M365TeamsAudit.csv";
                string fullFileName = filePath + fileName;
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"createdDateTime\",\"userPrincipalName\",\"applicationDisplayName\",\"ipAddress\",\"clientAppUsed\",\"additionalDetails\",\"operatingSystem\",\"browser\",\"countryOrRegion\",\"city\",\"tenantId\",\"timeStamp\"");



                    string webApiUrl = $"https://graph.microsoft.com/v1.0/auditLogs/signIns?$filter=startsWith(appDisplayName,'Microsoft Teams') and createdDateTime ge {startDate}";

                    while (webApiUrl != "" && webApiUrl != null)
                    {
                        string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                        dynamic data = JObject.Parse(result);
                        // Recurrent graph query while there is a nextLink value
                        // The next query is set to empty value, and if some value comes in the response, it will be assigned.
                        webApiUrl = "";
                        foreach (JProperty child in data.Properties())
                        {
                            if (child.Name == "@odata.nextLink")
                            {
                                webApiUrl = child.Value.ToString();
                            }
                        }

                        dynamic values = data.value;

                        foreach (dynamic value in values)
                        {
                            output.WriteLine($"\"{value.createdDateTime}\",\"{value.userPrincipalName}\",\"{value.appDisplayName}\",\"{value.ipAddress}\",\"{value.clientAppUsed}\",\"{value.status.additionalDetails}\",\"{value.deviceDetail.operatingSystem}\",\"{value.deviceDetail.browser}\",\"{value.location.countryOrRegion}\",\"{value.location.city}\",\"{tenantId}\",\"{timeStamp}\"");
                        }
                        counter = counter + values.Count;
                        log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");
                    }
                    output.Close();
                    log.LogInformation("Finished M365TeamsAudit report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365TeamsAudit report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365PowerBIAuditData(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365PowerBIAudit report");

                int counter = 0;
                string startDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string tenantId = Common.GetSecret("TenantID", log).Value;
                string filePath = "C:/home/data/";
                string fileName = "M365PowerBIAudit.csv";
                string fullFileName = filePath + fileName;
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"createdDateTime\",\"userPrincipalName\",\"applicationDisplayName\",\"ipAddress\",\"clientAppUsed\",\"status\",\"serviceDetail\",\"location\",\"tenantId\",\"timeStamp\"");


                    string webApiUrl = $"https://graph.microsoft.com/v1.0/auditLogs/signIns?$filter=startsWith(appDisplayName,'Microsoft Power BI') and createdDateTime ge {startDate}";

                    while (webApiUrl != "" && webApiUrl != null)
                    {
                        string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                        dynamic data = JObject.Parse(result);
                        // Recurrent graph query while there is a nextLink value
                        // The next query is set to empty value, and if some value comes in the response, it will be assigned.
                        webApiUrl = "";
                        foreach (JProperty child in data.Properties())
                        {
                            if (child.Name == "@odata.nextLink")
                            {
                                webApiUrl = child.Value.ToString();
                            }
                        }

                        dynamic values = data.value;

                        foreach (dynamic value in values)
                        {
                            output.WriteLine($"\"{value.createdDateTime}\",\"{value.userPrincipalName}\",\"{value.appDisplayName}\",\"{value.ipAddress}\",\"{value.clientAppUsed}\",\"{value.status.additionalDetails}\",\"{value.deviceDetail.operatingSystem}\",\"{value.location.countryOrRegion}\",\"{tenantId}\",\"{timeStamp}\"");
                        }
                        counter = counter + values.Count;

                        log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");

                    }
                    output.Close();
                    log.LogInformation("Finished M365PowerBIAudit report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365PowerBIAudit report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365ExchangeDistributionList(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365ExchangeDistributionList report");

                int counter = 0;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");

                string filePath = "C:/home/data/";
                string fileName = "M365ExchangeDistributionList.csv";
                string fullFileName = filePath + fileName;
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"description\",\"mailenabled\",\"securityenabled\",\"tenantId\",\"timestamp\"");

                    string webApiUrl = $"https://graph.microsoft.com/beta/groups?$select=organizationId,displayName,mailEnabled,securityEnabled";

                    while (webApiUrl != "" && webApiUrl != null)
                    {
                        string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                        dynamic data = JObject.Parse(result);
                        // Recurrent graph query while there is a nextLink value
                        // The next query is set to empty value, and if some value comes in the response, it will be assigned.
                        webApiUrl = "";
                        foreach (JProperty child in data.Properties())
                        {
                            if (child.Name == "@odata.nextLink")
                            {
                                webApiUrl = child.Value.ToString();
                            }
                        }

                        dynamic values = data.value;

                        foreach (dynamic value in values)
                        {
                            try
                            {

                                output.WriteLine($"\"{value.displayName}\",\"{value.mailEnabled}\",\"{value.securityEnabled}\",\"{value.organizationId}\",\"{timeStamp}\"");
                            }
                            catch (Exception e)
                            {
                                log.LogError(e.Message);
                                log.LogWarning($"\"{value.displayName}\",\"{value.mailEnabled}\",\"{value.securityEnabled}\",\"{value.organizationId}\",\"{timeStamp}\"");
                            }
                        }
                        counter = counter + values.Count;

                        log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");
                    }
                    output.Close();
                    log.LogInformation("Finished M365ExchangeDistributionList report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365ExchangeDistributionList report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365AllUsersType(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365AllUsersType report");

                int counter = 0;
                string tenantId = Common.GetSecret("TenantID", log).Value;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = "C:/home/data/";
                string fileName = "M365AllUsersType.csv";
                string fullFileName = filePath + fileName;
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"id\",\"accountEnabled\",\"createdDateTime\",\"deletedDateTime\",\"displayName\",\"mail\",\"usageLocation\",\"userPrincipalName\",\"externalUserState\",\"userType\",\"country\",\"department\",\"timestamp\",\"lastSignInDateTime\",\"lastNonInteractiveSignInDateTime\",\"tenantId\"");


                    string webApiUrl = $"https://graph.microsoft.com/beta/users?select=id,deletedDateTime,accountEnabled,createdDateTime,displayName,mail,usageLocation,userPrincipalName,externalUserState,userType,country,department,signInActivity,&top=999";

                    while (webApiUrl != "" && webApiUrl != null)
                    {
                        string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                        dynamic data = JObject.Parse(result);
                        // Recurrent graph query while there is a nextLink value
                        // The next query is set to empty value, and if some value comes in the response, it will be assigned.
                        webApiUrl = "";
                        foreach (JProperty child in data.Properties())
                        {
                            if (child.Name == "@odata.nextLink")
                            {
                                webApiUrl = child.Value.ToString();
                            }
                        }

                        dynamic values = data.value;

                        foreach (dynamic value in values)
                        {

                            string lastSignInDateTime = "";
                            string lastNonInteractiveSignInDateTime = "";
                            // Those values doesn't always come in the JSON; I the property exists, I assign them and in not,are set to 0, which means 'user never loged'.
                            if (value.ContainsKey("signInActivity"))
                            {
                                lastSignInDateTime = value.signInActivity.lastSignInDateTime;
                                lastNonInteractiveSignInDateTime = value.signInActivity.lastNonInteractiveSignInDateTime;
                            }
                            else
                            {
                                lastSignInDateTime = "";
                                lastNonInteractiveSignInDateTime = "";
                            }
                            output.WriteLine($"\"{value.id}\",\"{value.accountEnabled}\",\"{value.createdDateTime}\",\"{value.deletedDateTime}\",\"{value.displayName}\",\"{value.mail}\",\"{value.usageLocation}\",\"{value.userPrincipalName}\",\"{value.externalUserState}\",\"{value.userType}\",\"{value.country}\",\"{value.department}\",\"{timeStamp}\",\"{lastSignInDateTime}\",\"{lastNonInteractiveSignInDateTime}\",\"{tenantId}\"");
                        }
                        counter = counter + values.Count;

                        log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");
                    }
                    output.Close();
                    log.LogInformation("Finished M365AllUsersType report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365AllUsersType report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365LicensesAcquiredRaw(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating GetM365LicensesAcquiredRaw report");
                int counter = 0;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = "C:/home/data/";
                string fileName = "M365LicensesAcquiredRaw.csv";
                string fullFileName = filePath + fileName;
                string tenantId = Common.GetSecret("TenantID", log).Value;



                // Opening the result file for writing
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"capabilityStatus\",\"consumedUnits\",\"prepaidUnits\",\"suspendedUnits\",\"warningUnits\",\"skuId\",\"skuPartNumber\",\"tenantId\",\"timeStamp\"");

                    string webApiUrl = $"https://graph.microsoft.com/v1.0/subscribedSkus";

                    while (webApiUrl != "" && webApiUrl != null)
                    {
                        string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                        dynamic data = JObject.Parse(result);
                        // Recurrent graph query while there is a nextLink value
                        // The next query is set to empty value, and if some value comes in the response, it will be assigned.
                        webApiUrl = "";
                        foreach (JProperty child in data.Properties())
                        {
                            if (child.Name == "@odata.nextLink")
                            {
                                webApiUrl = child.Value.ToString();
                            }
                        }

                        //Convert the values into a dynamic list and write the needed values in the file
                        dynamic values = data.value;
                        foreach (dynamic value in values)
                        {
                            output.WriteLine($"\"{value.capabilityStatus}\",\"{value.consumedUnits}\",\"{value.prepaidUnits.enabled}\",\"{value.prepaidUnits.suspended}\",\"{value.prepaidUnits.warning}\",\"{value.skuId}\",\"{value.skuPartNumber}\",\"{tenantId}\",\"{timeStamp}\"");
                        }

                        // Just debug information. To convert into logging info
                        counter = counter + values.Count;

                        log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");
                    }
                    output.Close();
                    log.LogInformation("Finished GetM365LicensesAcquiredRaw report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating GetM365LicensesAcquiredRaw report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365SecureScoreRecomendations(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365SecureScoreRecomendations report");

                int counter = 0;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = "C:/home/data/";
                string fileName = "M365SecureScoreRecomendations.csv";
                string fullFileName = filePath + fileName;

                // Opening the result file for writing
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"controlCategory\",\"controlName\",\"description\",\"score\",\"implementationStatus\",\"scoreInPercentage\",\"isApplicable\",\"controlState\",\"count\",\"lastSynced\",\"isEnforced\",\"tenantId\",\"timeStamp\"");

                    string webApiUrl = $"https://graph.microsoft.com/v1.0/security/secureScores?$top=1";
                    string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                    dynamic data = JObject.Parse(result);

                    //Convert the values into a dynamic list and write the needed values in the file
                    string tenantId = Common.GetSecret("TenantID", log).Value;

                    dynamic values = data.value[0].controlScores;

                    foreach (dynamic value in values)
                    {
                        string description = value.description;
                        description = description.Replace("\n", "");
                        description = description.Replace("\r", "");

                        output.WriteLine($"\"{value.controlCategory}\",\"{value.controlName}\",\"{description}\",\"{value.score}\",\"{value.implementationStatus}\",\"{value.scoreInPercentage}\",\"{value.IsApplicable}\",\"{value.controlState}\",\"{value.count}\",\"{value.lastSynced}\",\"{value.IsEnforced}\",\"{tenantId}\",\"{timeStamp}\"");
                    }

                    // Just debug information. To convert into logging info
                    counter = counter + values.Count;

                    log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");

                    output.Close();
                    log.LogInformation("Finished M365SecureScoreRecomendations report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365SecureScoreRecomendations report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365SecureScoreDetails(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365SecureScoreDetails report");

                int counter = 0;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = "C:/home/data/";
                string fileName = "M365SecureScoreDetails.csv";
                string fullFileName = filePath + fileName;

                // Opening the result file for writing
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"tenantId\",\"activeUserCount\",\"createdDateTime\",\"currentScore\",\"licensedUserCount\",\"maxScore\",\"allTenantsAverageScore\",\"SeatNumberAverageScore\",\"seatNumberLowerValue\",\"seatNumberUpperValue\",\"timeStamp\"");

                    string webApiUrl = $"https://graph.microsoft.com/v1.0/security/secureScores?$top=1";
                    string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                    SecureScore data = JsonConvert.DeserializeObject<SecureScore>(result);
                    if (data.Values.Count > 0)
                    {
                        SecureScore.Value value = data.Values[0];

                        if (value.AverageComparativeScores.Count > 1)
                        {
                            output.WriteLine($"\"{value.AzureTenantId}\",\"{value.ActiveUserCount}\",\"{value.CreatedDateTime}\",\"{value.CurrentScore}\",\"{value.LicensedUserCount}\",\"{value.MaxScore}\",\"{value.AverageComparativeScores[0].AverageScore}\",\"{value.AverageComparativeScores[1].AverageScore}\",\"{value.AverageComparativeScores[1].SeatSizeRangeLowerValue}\",\"{value.AverageComparativeScores[1].SeatSizeRangeUpperValue}\",\"{timeStamp}\"");
                        }
                        else if (value.AverageComparativeScores.Count == 1)
                        {
                            output.WriteLine($"\"{value.AzureTenantId}\",\"{value.ActiveUserCount}\",\"{value.CreatedDateTime}\",\"{value.CurrentScore}\",\"{value.LicensedUserCount}\",\"{value.MaxScore}\",\"{value.AverageComparativeScores[0].AverageScore}\",\"0\",\"0\",\"0\",\"{timeStamp}\"");
                        }
                        else
                        {
                            output.WriteLine($"\"{value.AzureTenantId}\",\"{value.ActiveUserCount}\",\"{value.CreatedDateTime}\",\"{value.CurrentScore}\",\"{value.LicensedUserCount}\",\"{value.MaxScore}\",\"0\",\"0\",\"0\",\"0\",\"{timeStamp}\"");
                        }
                    }
                    // Just debug information. To convert into logging info
                    counter = counter + data.Values.Count;
                    log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");

                    output.Close();
                    log.LogInformation("Finished M365SecureScoreDetails report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365SecureScoreDetails report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task EnableM365ActivityLog(string tenantId, string accessToken, ILogger log)
        {
            try
            {
                string[] activityLogs = { "Audit.General", "Audit.SharePoint", "Audit.AzureActiveDirectory", "Audit.Exchange" };

                foreach (string activityLog in activityLogs)
                {
                    log.LogInformation("Enabling " + activityLog + " Log");
                    string webApiUrl = $"https://manage.office.com/api/v1.0/" + tenantId + "/activity/feed/subscriptions/start?contentType=" + activityLog;
                    string result = await Common.GetAPIDataPost(accessToken, webApiUrl, log);

                    dynamic data = JsonConvert.DeserializeObject(result);
                    if (data.error.code == "AF20024")
                    {
                        string error = data.error.message;
                        log.LogInformation(error);
                    }
                    else if (data.status == "enabled")
                    {
                        string status = data.status;
                        log.LogInformation(status);
                    }
                    else
                    {
                        log.LogInformation("Nothing happened :(");
                    }
                }
            }

            catch (Exception e)
            {
                log.LogError("Error generating M365SecureScoreDetails report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365ActivityLog(ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365 ActivityLog report");


                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = "C:/home/data/";
                string fileName = "M365ActivityLog.csv";
                string fullFileName = filePath + fileName;

                string appId = Common.GetSecret("ApplicationID", log).Value;
                string secret = Common.GetSecret("ApplicationSecret", log).Value;
                string tenantId = Common.GetSecret("TenantID", log).Value;

                // string appId = "9449875b-23e2-47d5-8eb6-6885268088a1";
                // string secret = ".zD8Q~OYJH6Xs51r8KunMsAOZM1cLjP3etbNibMk";
                // string tenantId = "ffcd3259-778e-4973-a330-1881408cc8bb";

                string accessToken = Common.GetBearer(tenantId, appId, secret, "https://manage.office.com/", log);

                string[] activityLogs = { "Audit.General", "Audit.SharePoint", "Audit.AzureActiveDirectory", "Audit.Exchange" };

                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"activityLogId\",\"creationTime\",\"m365Tenant\",\"m365UserPrincipal\",\"userType\",\"operation\",\"resultStatus\",\"workload\"");

                    //Enabling ActivityLog feed
                    await EnableM365ActivityLog(tenantId, accessToken, log);

                    foreach (var activityLog in activityLogs)
                    {
                        string webApiUrl = $"https://manage.office.com/api/v1.0/" + tenantId + "/activity/feed/subscriptions/content?contentType=" + activityLog;

                        string result = await Common.GetAPIDataGet(accessToken, webApiUrl, log);

                        dynamic data = JsonConvert.DeserializeObject(result);
                        foreach (dynamic value in data)
                        {
                            string contentUri = value.contentUri.ToString();
                            string activityLogContent = await Common.GetAPIDataGet(accessToken, contentUri, log);
                            dynamic activityLogContentJSON = JsonConvert.DeserializeObject(activityLogContent);
                            foreach (dynamic activityLogItem in activityLogContentJSON)
                            {
                                try
                                {
                                    string activityLogId = activityLogItem.Id.ToString() ?? "";
                                    string creationTime = activityLogItem.CreationTime.ToString();
                                    string m365Tenant = activityLogItem.OrganizationId.ToString();
                                    string m365UserPrincipal = activityLogItem.UserId.ToString();
                                    string m365UserType = activityLogItem.UserType.ToString();
                                    string operation = activityLogItem.Operation.ToString();
                                    string resultStatus = activityLogItem.ResultStatus.ToString();
                                    string workload = activityLogItem.Workload.ToString();

                                    output.WriteLine(activityLogId + "," + creationTime + "," + m365Tenant + "," + m365UserPrincipal + "," + m365UserType + "," + operation + "," + resultStatus + "," + workload);
                                }
                                catch
                                {
                                    // No need to do anything in case of exception. Exceptions are provocated by system events without relevance
                                }
                            }
                        }
                    }

                    output.Close();
                    log.LogInformation("Finished  M365 ActivityLog report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365 ActivityLog report");
                log.LogWarning(e.Message);

            }
        }
        public static async Task GetM365SharedMailboxes(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365ExchangeAttributes report");

                int counter = 0;
                string tenantId = Common.GetSecret("TenantID", log).Value;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = "C:/home/data/";
                string fileName = "M365ExchangeAttributes.csv";
                string fullFileName = filePath + fileName;
                using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
                {
                    //writing the header of the file
                    output.WriteLine("\"DisplayName\",\"UPN\",\"RetentionHold\",\"LitigationHold\",\"RecipientType\",\"TimeStamp\"");

                    string hostName = Environment.GetEnvironmentVariable("PSAPI");
                    string webApiUrl = $"HTTPS://{hostName}.azurewebsites.net/api/M365SharedMailboxes";                    

                    while (webApiUrl != "" && webApiUrl != null)
                    {
                        string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                        dynamic data = JObject.Parse(result);
                        // Recurrent graph query while there is a nextLink value
                        // The next query is set to empty value, and if some value comes in the response, it will be assigned.
                        webApiUrl = "";
                        foreach (JProperty child in data.Properties())
                        {
                            if (child.Name == "@odata.nextLink")
                            {
                                webApiUrl = child.Value.ToString();
                            }
                        }

                        dynamic values = data.data;

                        foreach (dynamic value in values)
                        {
                            //string displayName = value.DisplayName.replace(",", ";");
                            output.WriteLine($"\"{value.DisplayName}\",\"{value.WindowsEmailAddress}\",\"{value.RetentionHoldEnabled}\",\"{value.LitigationHoldEnabled}\",\"{value.RecipientTypeDetails}\",\"{timeStamp}\"");
                        }
                        counter = counter + values.Count;

                        log.LogInformation($"{DateTime.Now.ToString()} - Items recovered: {counter}");
                    }
                    output.Close();
                    log.LogInformation("Finished M365ExchangeAttributes report generation");
                    await Common.UploadBlob("m365", fileName, log);
                }
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365AllUsersType report");
                log.LogWarning(e.Message);

            }
        }
    }
}

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

