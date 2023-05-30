using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Azure.Identity;
using Azure.Storage.Sas;
using Azure.Storage.Blobs.Specialized;
using Azure.Security.KeyVault.Secrets;
using System.Data.SqlClient;
using System.Data;

namespace CloudInform
{
    public static class M365DataImport
    {
        [FunctionName("M365DataImport")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
               

                // Waking up Serverless database if paused
                Common.WakeUpSQLDatabase(log);

                //   GenerateSASToken(); 
                //   Common.GenerateSASToken(log);       //Remove this line in following reviews. Luise, 2022-06-01

                Common.RunStoredProcedure("sp_loadfromdatalake_m365_all_users_type", log);
                if (Environment.GetEnvironmentVariable("M365") == "1")
                {
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_exchange_activity", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_exchange_distribution_lists", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_exchange_usage", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_groups_detail", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_licenses_acquired", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_licenses_assigned_user", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_onedrive_share_information", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_onedrive_usage", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_powerbi_audit", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_securescore_details", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_securescore_recomendations", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_sharepoint_sites", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_sharepoint_user_activity", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_teams_audit", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_teams_user_activity", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_yammer_user_activity", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_activity_log", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_exchange_attributes", log);
                    Common.RunStoredProcedure("sp_loadfromdatalake_m365_activations_user_detail", log);
                }
                return new OkObjectResult("M365DataImport Function Finished");
            }
            catch (Exception e)
            {
                log.LogError($"Error executing M365DataImport Function");
                log.LogError($"{e.Message}");
                throw ;
            }
        }
    }
}
