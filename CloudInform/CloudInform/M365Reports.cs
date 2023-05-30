using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInform
{
    internal class M365Reports
    {
        public static async Task GetM365ExchangeUsage(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365exchangeUsage report");

                string fileName = "M365exchangeUsage.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getMailboxUsageDetail(period='D180')";

                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);
                log.LogInformation("Finished M365exchangeUsage report generation");
                await Common.UploadBlob("m365", fileName, log);

            }
            catch (Exception e)
            {
                log.LogError("Error generating M365exchangeUsage report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365ExchangeActivity(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365ExchangeActivity report");

                string fileName = "M365ExchangeActivity.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getEmailActivityUserDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);


                Common.SaveCSV(result, fileName, log);
                log.LogInformation("Finished M365ExchangeActivity report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365ExchangeActivity report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365SharePointSites(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating GetM365SharePointSites report");

                string fileName = "M365SharePointSites.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getSharePointSiteUsageDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished GetM365SharePointSites report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating GetM365SharePointSites report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365GroupsDetail(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365GroupsDetail report");

                string fileName = "M365GroupsDetail.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getOffice365GroupsActivityDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365GroupsDetail report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365GroupsDetail report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365OneDriveUsage(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365OneDriveUsage report");

                string fileName = "M365OneDriveUsage.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getOneDriveUsageAccountDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365OneDriveUsage report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365OneDriveUsage report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365OneDriveShareInformation(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365OneDriveShareInformation report");

                string fileName = "M365OneDriveShareInformation.csv";
                string webApiUrl = "https://graph.microsoft.com/v1.0/reports/getOneDriveActivityUserDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                
                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365OneDriveShareInformation report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365OneDriveShareInformation report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365TeamsUserActivity(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365TeamsUserActivity report");

                string fileName = "M365TeamsUserActivity.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getTeamsUserActivityUserDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                
                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365TeamsUserActivity report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365TeamsUserActivity report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365SharePointUserActivity(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365SharePointUserActivity report");

                string fileName = "M365SharePointUserActivity.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getSharePointActivityUserDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365SharePointUserActivity report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365SharePointUserActivity report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365YammerUserActivity(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365YammerUserActivity report");

                string fileName = "M365YammerUserActivity.csv";
                string webApiUrl = "https://graph.microsoft.com/v1.0/reports/getYammerActivityUserDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);
                
                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365YammerUserActivity report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365YammerUserActivity report");
                log.LogWarning(e.Message);
                
            }
        }
        public static async Task GetM365LicensesAssignedUser(string accessToken, ILogger log)
        {
            try
            {
                log.LogInformation("Generating M365LicensesAssignedUser report");

                string fileName = "M365LicensesAssignedUser.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getOffice365ActiveUserDetail(period='D180')";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365LicensesAssignedUser report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365LicensesAssignedUser report");
                log.LogWarning(e.Message);
                
            }
        }

        public static async Task GetM365ActivationsUserDetail(string accessToken, ILogger log)
        //Reference: https://learn.microsoft.com/en-us/graph/api/reportroot-getoffice365activationsuserdetail?view=graph-rest-beta
        {
            try
            {
                log.LogInformation("Generating M365ActivationsUserDetail report");

                string fileName = "M365ActivationsUserDetail.csv";
                string webApiUrl = "https://graph.microsoft.com/beta/reports/getOffice365ActivationsUserDetail?$format=text/csv";
                string result = await Common.GetM365Data(accessToken, webApiUrl, log);

                result = Common.RemoveCommas(result, log);
                result = Common.AddTenantId(result, log);
                result = Common.AddTimestamp(result, log);

                Common.SaveCSV(result, fileName, log);

                log.LogInformation("Finished M365ActivationsUserDetail report generation");
                await Common.UploadBlob("m365", fileName, log);
            }
            catch (Exception e)
            {
                log.LogError("Error generating M365ActivationsUserDetail report");
                log.LogWarning(e.Message);

            }
        }
    }
}
