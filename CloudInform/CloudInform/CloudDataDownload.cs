using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Security.KeyVault.Secrets;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Collections.Generic;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using System.Web;

namespace CloudInform
{
	public static class CloudDataDownload
	{
		[FunctionName("CloudDataDownload")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			try
			{

				//Calculating the starting and ending date for getting data
				int daysBack = int.Parse(Environment.GetEnvironmentVariable("DaysBack"));  //Environment variables are string and I need an Int
				if (daysBack > 0) { daysBack *= (-1); } // DaysBack variable should be negative. This makes the sign of the value in the variable irrelevant.
				DateTime startDate;
				DateTime endDate;
				string startDateParameter = req.Query["startdate"].ToString();
				string endDateParameter = req.Query["endDate"].ToString();
				if ((startDateParameter != null )&&(startDateParameter !="") && (endDateParameter != null) && (endDateParameter != ""))
				{
					try 
					{ 
						startDate = DateTime.Parse(startDateParameter); 
						endDate= DateTime.Parse(endDateParameter);
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


				// **** Starting Azure Legacy (With API KEY for EA Enrollments *****
				if (Environment.GetEnvironmentVariable("AzureEALegacy") == "1")
				{
					AzureAPI.GetAzureData(startDate, endDate, log);
					//await RBAC.GetRBACPermissions(log);
					await AzureLegacy.GetAzureLegacyData(startDate, endDate,log);
				}

                // **** Starting Azure CSP  *****
                if (Environment.GetEnvironmentVariable("AzureCSP") == "1")
                {
                    await AzureCSP.GetAzureCSPDataAsync(startDate, endDate, log);
                }

                // **** Starting Google Cloud Platform data recovery from BigData Database *****  * 							
                if (Environment.GetEnvironmentVariable("GCP") == "1")
				{
					log.LogInformation("Starting Function - Recovering GCP UsageDetails Data");
					for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
					{
						log.LogInformation("Querying BigQuery for day " + currentDate.ToString("yyyy-MM-dd"));
						string fileName = GCP.GetBigqueryData(currentDate);

						log.LogInformation("BigQuery data processed.");

						log.LogInformation("Uploading gpc data from day: " + currentDate.ToString("yyyy-MM-dd"));
						await Common.UploadBlob("gcp", fileName, log);
						log.LogInformation($"GCP data from day: {currentDate.ToString("yyyy-MM-dd")} uploaded");

					}
					log.LogInformation("Querying BigQuery for refunds " + endDate.AddDays(-180).ToString("yyyy-MM-dd"));
			//Luis Note: Do not remove this commented code yet. Not needed with new calculations, but you never know (2022-11-04).
				// Processing past Refunds
				//	string fileNameRefunds = GCP.GetBigqueryDataRefunds(endDate, log);
				//	await Common.UploadBlob("gcp", fileNameRefunds, log);
				//	log.LogInformation($"GCP refund data from day: {endDate.AddDays(-180).ToString("yyyy-MM-dd")} uploaded");
				}

				log.LogInformation($"Finished Function UsageDetails at {endDate}");
				return new OkObjectResult("Operation Finished");

			}
			catch (Exception e)
			{
				log.LogError(e.Message);
				return new OkObjectResult("Error: " + e.Message);
			}
		}
	}
}

