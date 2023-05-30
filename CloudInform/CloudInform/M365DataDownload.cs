using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Threading;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace CloudInform
{
    public static class M365DataDownload
    {
        [FunctionName("M365DataDownload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting M365Reports_Download function");
                string m365Token = await Common.GetM365Token(log);

                //This function should always be executed because shares info with cloud dashboard

                await M365Graph.GetM365AllUsersType(m365Token, log);

                if (Environment.GetEnvironmentVariable("M365") == "1")
                {
                    // Download Usage Reports (they are raw CSV)
                    await M365Reports.GetM365ExchangeUsage(m365Token, log);
                    await M365Reports.GetM365ExchangeActivity(m365Token, log);
                    await M365Reports.GetM365SharePointSites(m365Token, log);
                    await M365Reports.GetM365GroupsDetail(m365Token, log);
                    await M365Reports.GetM365OneDriveUsage(m365Token, log);
                    await M365Reports.GetM365OneDriveShareInformation(m365Token, log);
                    await M365Reports.GetM365TeamsUserActivity(m365Token, log);
                    await M365Reports.GetM365SharePointUserActivity(m365Token, log);
                    await M365Reports.GetM365YammerUserActivity(m365Token, log);
                    await M365Reports.GetM365LicensesAssignedUser(m365Token, log);
                    await M365Reports.GetM365ActivationsUserDetail(m365Token, log);                    

                    //Download Data from Graph (Json that must be processed)
                    await M365Graph.GetM365SharedMailboxes(m365Token, log);
                    await M365Graph.GetM365ExchangeDistributionList(m365Token, log);
                    await M365Graph.GetM365LicensesAcquiredRaw(m365Token, log);
                    await M365Graph.GetM365SecureScoreDetails(m365Token, log);
                    await M365Graph.GetM365SecureScoreRecomendations(m365Token, log);
                    await M365Graph.GetM365PowerBIAuditData(m365Token, log);
                    await M365Graph.GetM365TeamsAudit(m365Token, log);
                    await M365Graph.GetM365ActivityLog(log);
                }

                log.LogInformation("M365Reports_Download function finished");
                return new OkObjectResult("Operation Finished");
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                throw;
            }
        }        
    }
}
