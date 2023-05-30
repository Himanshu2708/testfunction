using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Security.KeyVault.Secrets;
using System.Data.SqlClient;
using System.Data;
using Azure.Storage.Blobs;
using Azure.Identity;
using Azure.Storage.Blobs.Models;
using CloudInform.CSPAPIWrapper;

namespace CloudInform

{
    public static class CloudDataImport
    {
        [FunctionName("CloudDataImport")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Checking for online Database");
                Common.WakeUpSQLDatabase(log);

                log.LogInformation("Starting Import Cloud Data Into SQL Function.");

                //Calculating the starting and ending date for getting data
                int daysBack = int.Parse(Environment.GetEnvironmentVariable("DaysBack"));  //Environment variables are string and I need an Int
                if (daysBack > 0) { daysBack *= (-1); } // DaysBack variable should be negative. This makes the sign of the value in the variable irrelevant.
                DateTime startDate;
                DateTime endDate;
                string startDateParameter = req.Query["startdate"].ToString();
                string endDateParameter = req.Query["endDate"].ToString();
                if ((startDateParameter != null) && (startDateParameter != "") && (endDateParameter != null) && (endDateParameter != ""))
                {
                    try
                    {
                        startDate = DateTime.Parse(startDateParameter);
                        endDate = DateTime.Parse(endDateParameter);
                    }
                    catch
                    {
                        log.LogError($"Invalid values on parameters. startDate: {startDateParameter} |endDate: {endDateParameter}");
                        log.LogWarning("verify that the parameters are in format yyyy-MM-dd");
                        log.LogWarning("Sample request: https://xxxxx/api/DownloadUsageDetails?startdate=2022-01-01&enddate=2022-02-28");
                        throw;
                    }
                }
                else
                {
                    if ((startDateParameter != null) && (startDateParameter != ""))
                    {
                        try
                        {
                            startDate = DateTime.Parse(startDateParameter);
                            endDate = DateTime.Now;
                        }
                        catch
                        {
                            log.LogError("Invalid values on parameter. startDate: {startDateParameter}");
                            log.LogWarning("verify that the parameters are in format yyyy-MM-dd");
                            log.LogWarning("Sample request: https://xxxxx/api/DownloadUsageDetails?startdate=2022-01-01");
                            throw;
                        }
                    }
                    else
                    {
                        startDate = DateTime.Now.AddDays(daysBack);
                        endDate = DateTime.Now;
                    }

                }
                log.LogInformation($"Processing data from {startDate} to {endDate}");

                // Waking up Serverless database if paused
                Common.WakeUpSQLDatabase(log);


                // **** Starting with Azure EA TEMP_UsageDetails
                if (Environment.GetEnvironmentVariable("AzureEALegacy") == "1")
                {
                    log.LogInformation("Starting Azure UsageDetails Legacy Data Ingestion UsageDetails");
                    string enrollmentsList = Environment.GetEnvironmentVariable("Enrollment");
                    log.LogInformation($"Found Enrollments: {enrollmentsList}");
                    //enrollmentsList = enrollmentsList + ",";
                    string[] enrollments = enrollmentsList.Split(',');
                    foreach (string enrollment in enrollments)
                    {
                        try
                        {
                            DateTime currentDate = startDate;
                            while (currentDate <= endDate)
                            {
                                DateTime lastMonthDate = currentDate.AddDays(-31);
                                string fileDate = currentDate.ToString("yyyyMMdd");
                                string lastMonthFileDate = lastMonthDate.ToString("yyyyMMdd");

                                log.LogInformation($"Processing files from enrollment: {enrollment}");
                                Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_azure_cost_detail_usage '{fileDate}','{enrollment}'", log);
                                currentDate = currentDate.AddDays(1);

                                // Consumption and marketplace last month
                                string consumptionPeriod = currentDate.ToString("yyyyMM");
                                log.LogInformation($"Processing Consumption Summary for enrollment: {enrollment} and date {fileDate}");
                                Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_azure_consumption_summary '{fileDate}','{enrollment}'", log);

                                log.LogInformation($"Processing Marketplace information for enrollment: {enrollment} and date {fileDate}");
                                Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_azure_marketplace_summary '{fileDate}','{enrollment}'", log);

                                // Consumption and marketplace this month
                                log.LogInformation($"Processing Consumption Summary for enrollment: {enrollment} and date {lastMonthFileDate}");
                                Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_azure_consumption_summary '{lastMonthFileDate}','{enrollment}'", log);

                                log.LogInformation($"Processing Marketplace information for enrollment: {enrollment} and date {lastMonthFileDate}");
                                Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_azure_marketplace_summary '{lastMonthFileDate}','{enrollment}'", log);

                            }
                        }
                        catch (Exception e)
                        {
                            log.LogWarning($"{e.Message}");
                        }
                        
                    }
                }

                // **** Starting with Azure CSP TEMP_UsageDetails
                if (Environment.GetEnvironmentVariable("AzureCSP") == "1")
                {
                    log.LogInformation("Starting Azure CSP UsageDetails Data Ingestion.");
                    try
                    {
                        DateTime currentDate = startDate;
                        while (currentDate <= endDate)
                        {
                            string fileDate = currentDate.ToString("yyyyMMdd");
                            log.LogInformation($"Processing CSP data for date: {currentDate}");

                            string fileName = $"AZ-CSP-Usagedetails-{fileDate}.csv";
                            bool fileExists = await Common.FileExists("azure",fileName, log);
                            if (fileExists)
                            {
                                Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_azureCSP_cost_detail_usage '{fileDate}'", log);
                            }
                            else
                            {
                                log.LogWarning($"No data for day {currentDate}");
                            }
                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogWarning($"{e.Message}");
                    }
                }
                    // **** Starting with GCP TEMP_UsageDetails
                    if (Environment.GetEnvironmentVariable("GCP") == "1")
                {
                    try
                    {
                        log.LogInformation("Starting GCP UsageDetails Data Ingestion to TEMP Table");
                        DateTime currentDate = startDate;
                        while (currentDate <= endDate)
                        {
                            log.LogInformation($"Processing GCP files for date: {currentDate}");
                            Common.RunSQLQuery($"exec sp_loadfromdatalake_cloud_gcp_cost_detail_usage '{currentDate}'", log);
                            currentDate = currentDate.AddDays(1);

                        }

                        //Generating Consumption Period (last 6 months)
                        for (int i = -6; i == 0; i++)
                        {
                            string consumptionPeriod = currentDate.AddMonths(-i).ToString("yyyyMM");
                            log.LogInformation($"Processing Consumption Summary for Consumption Period {consumptionPeriod}");
                            Common.RunSQLQuery($"exec sp_import_cloud_gcp_summary_consumption '{consumptionPeriod}'", log);
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogWarning($"{e.Message}");
                    }
                }

                return new OkObjectResult("Function Finished");
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
            finally
            {

            }


        }
    }
}
