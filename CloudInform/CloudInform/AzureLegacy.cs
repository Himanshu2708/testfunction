using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CloudInform
{
    internal class AzureLegacy
    {
		private static async Task<string> GenerateAzureUsageDetailsLegacy(string enrollment, string token, string date, ILogger log)
		{
			try
			{
				log.LogInformation($"Starting Data Generation for enrollment: {enrollment} day: {date}");

				var blobPath = "";
				using (var httpClient = new HttpClient())
				{
					string result="";
					httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
					var data = new StringContent("JSON", Encoding.UTF8, "application/json");
					int throttling = 1;
					while (throttling==1)
					{ 
						var response = await httpClient.PostAsync("https://consumption.azure.com/v3/enrollments/" + enrollment + "/usagedetails/submit?startTime=" + date + "&endTime=" + date, data);
						result = await response.Content.ReadAsStringAsync();
						string responseStatusCode=response.StatusCode.ToString();
						if (responseStatusCode == "TooManyRequests")
						{
							throttling = 1;
							log.LogWarning("Requests are being throttled; waiting 2 minutes");
							System.Threading.Thread.Sleep(120000);
						}
						else
						{
							throttling = 0;
						}
					}
					log.LogInformation(result.ToString());
				
					var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
					var pollUrl = dict["reportUrl"];
					var status = 0;
					do
					{
						var pollingJsonResult = await httpClient.GetStringAsync(pollUrl);
						var pollingResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(pollingJsonResult);
						status = int.Parse(pollingResult["status"]);

						switch (status)
						{
							case 1:
								log.LogInformation($"Waiting to start");
								break;
							case 2:
								log.LogInformation($"Processing data");
								break;
							case 3:
								log.LogInformation($"File ready to download in URL: {pollUrl}");
								break;
							case 5:
								log.LogInformation($"No data from date: {date}");
								throw new FormatException($"No data from date {date}");
						}

					
						if (status == 3)
						{
							blobPath = pollingResult["blobPath"];
						}
						System.Threading.Thread.Sleep(5000);
					}
					while (status != 3);
				}
				return blobPath;
			}
			catch
			{
				throw;
			}
		}

		private static string DownloadAzureUsageDetailsFileLegacy(string blob, string enrollment, string date,ILogger log)
		{
			try
			{
				log.LogInformation($"Starting file download for enrollment: {enrollment} day: {date}");

				string filePath = "C:/home/data/";
				string fileName = $"TEMP_AZ-Usagedetails-{enrollment}-{date}.csv";
				string fullFile = filePath + fileName;




				using (WebClient client = new WebClient())
				{
					log.LogInformation($"Downloading file: {fileName}");
					client.DownloadFile(blob, fullFile);
					log.LogInformation($"Downloaded file: {fileName}");
				}
				return fileName;
			}
			catch
			{
				throw;
			}
		}

		private static string ConvertAzureUsageDetailsFileLegacy(string fileName,ILogger log)
		{
			try
			{
				log.LogInformation($"Starting file format conversion for {fileName}");

				string filePath = "C:/home/data/";
				string fullFileName = filePath + fileName;

				string newFileName = fileName.Replace("TEMP_", "");
				string newFullFileName = filePath + newFileName;

				using (var input = File.OpenText(fullFileName))
				using (var output = new StreamWriter(newFullFileName))
				{
					string line;
					while (null != (line = input.ReadLine()))
					{
						line = line.Replace("#", " ");      //Replace possible hash symbol with space to avoid interferences
						line = line.Replace("\",  ", "¬");  //Replace delimieter string in Tag fields with ¬ 
						line = line.Replace("null,", "¬¬"); //Replace delimiter string in tags fields 'null,' with '¬¬'
						line = line.Replace(",  \"", "¬¬¬");//Replace delimiter strin in tags fields with ¬¬¬
						line = line.Replace(".com, ", "¬¬¬¬");//Replace delimiter strin in tags fields with ¬¬¬
						line = line.Replace(",", "#");      //Replace , per # to make a custom CSV
						line = line.Replace("¬¬", "null,"); //Replacing back '¬¬' with 'null,'
						line = line.Replace("¬", "\",  ");  //Replacing back json in tags delimiter string
						line = line.Replace("¬¬¬", ",  \"");//Replacing back ¬¬¬ in tags delimiter string
						line = line.Replace("¬¬¬¬", ".com, ");//Replacing back ¬¬¬ in tags delimiter string
						output.WriteLine(line);
					}
					output.Close();
					input.Close();
				}
				log.LogDebug($"Finished processing {newFileName}");
				log.LogDebug($"cleaning {fileName}");
				return newFileName;

			}
			catch
			{
				throw;
			}
			finally
			{
				if (File.Exists($"c:/home/data/{fileName}"))
				{

					File.Delete($"c:/home/data/{fileName}");  //Whatever happens, If the original files exists, I remove the original file.
				}

			}
		}


		private static async Task DownloadAzureConsumptionSummaryLegacy(string enrollment, string ConsumptionPeriod, string token, ILogger log)
		{
			try
			{
				string url = "https://consumption.azure.com/v3/enrollments/" + enrollment + "/billingPeriods/" + ConsumptionPeriod + "/balancesummary";
				string path = "c:/home/data/";
				string fileName = $"AZ-consumptionsummary-{enrollment}-{ConsumptionPeriod}.csv";
				string fullFileName = $"{path}{fileName}";

				StreamWriter output = new StreamWriter(fullFileName, false, Encoding.Unicode);
				string result = await Common.GetAPIDataGet(token, url, log);

				output.WriteLine("enrollment#billingPeriodId#currencyCode#beginingBalance#endingBalance#newPurchases#adjustments#utilized#serviceOverage" +
					"#chargesBilledSeparately#totalOverage#totalUsage#azureMarketplaceServiceCharges#newPurchaseDetails#adjustmentDetails#billingPeriodStart#billingPeriodEnd#balanceDeduct");

				dynamic data = JArray.Parse(result);

				foreach (dynamic item in data)
				{
					output.WriteLine($"{enrollment}" +
						$"#{item.billingPeriodId}" +
						$"#{item.currencyCode}" +
						$"#{item.beginningBalance}" +
						$"#{item.endingBalance}" +
						$"#{item.newPurchases}" +
						$"#{item.adjustments}" +
						$"#{item.utilized}" +
						$"#{item.serviceOverage}" +
						$"#{item.chargesBilledSeparately}" +
						$"#{item.totalOverage}" +
						$"#{item.totalUsage}" +
						$"#{item.azureMarketplaceServiceCharges}" +
						$"#{item.newPurchasesDetails}" +
						$"#{item.adjustmentDetails}" +
						$"#{item.billingPeriodStart}" +
						$"#{item.billingPeriodEnd}" +
						$"#{item.balanceDeduct}");
				}
				output.Close();
				await Common.UploadBlob("azure", fileName,log);
			}
			catch { }
		}

		private static async Task DownloadAzureMarketplaceSummaryLegacy(string enrollment, string ConsumptionPeriod, string token, ILogger log)
		{
			try
			{
				string today = DateTime.Now.ToString("yyyyMM");
				string url = "https://consumption.azure.com/v3/enrollments/" + enrollment + "/billingPeriods/" + ConsumptionPeriod + "/marketplacecharges";
				string path = "c:/home/data/";
				string fileName = $"AZ-marketplacesummary-{enrollment}-{ConsumptionPeriod}.csv";
				string fullFileName = $"{path}{fileName}";

				StreamWriter output = new StreamWriter(fullFileName, false, Encoding.Unicode);
				string result = await Common.GetAPIDataGet(token, url, log);
				output.WriteLine("enrollment#billingPeriod#date#usageStartDate#usageEndDate#departmentName#accountName" +
					"#subscriptionguid#subscriptionName#resourceGroup#offerName#publisherName#planName#instanceid#consumedQuantity" +
					"#resourceRate#extendedCost#unitoOfMeasure#meterid#tags#costcenter#ordernumber#additionalinfo#isRecurringCharge#cloud");

				dynamic data = JArray.Parse(result);

				foreach (dynamic item in data)
				{
					output.WriteLine($"{enrollment}" +
					$"#{ConsumptionPeriod}" +
					$"#{item.usageStartDate}" +
					$"#{item.usageStartDate}" +
					$"#{item.usageEndDate}" +
					$"#{item.department}" +
					$"#{item.accountName}" +
					$"#{item.subscriptionGuid}" +
					$"#{item.subscriptionName}" +
					$"#{item.resourceGroup}" +
					$"#{item.offerName}" +
					$"#{item.publisherName}" +
					$"#{item.planName}" +
					$"#{item.instanceId}" +
					$"#{item.consumedQuantity}" +
					$"#{item.resourceRate}" +
					$"#{item.extendedCost}" +
					$"#{item.unitOfMeasure}" +
					$"#{item.meterId}" +
					$"#{item.tags}" +
					$"#{item.costCenter}" +
					$"#{item.orderNumber}" +
					$"#{item.additionalInfo}" +
					$"#{item.isRecurringCharge}" +
					$"#AZ");

				}
				output.Close();
				await Common.UploadBlob("azure", fileName,log);
			}
			catch { }
		}

		public static async Task GetAzureLegacyData(DateTime startDate, DateTime endDate,ILogger log)
		{
			try
			{
				log.LogInformation("Starting Function - Recovering Azure UsageDetails Legacy Data");
				string enrollmentsList = Environment.GetEnvironmentVariable("Enrollment");
				log.LogInformation($"Found Enrollments: {enrollmentsList}");
				string[] enrollments = enrollmentsList.Split(',');

				// Doing a loop to get the data from all the enrollments.  
				foreach (string enrollment in enrollments)
				{
					log.LogInformation($"Processing enrollment: {enrollment}");
					string tokenName = $"EnrollmentAPIKey-{enrollment.Trim()}";

					string token = Common.GetSecret(tokenName, log).Value; //Get Secret return type KeyVaultSecret, but I only need the value.

					//Now the loop to download all the files in the last 'dayback' days and upload them to Azure Begins
					for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
					{
						// Azure Consumption Summary and Azure Marketplace data from last month
						string lastMonth = currentDate.AddDays(-31).ToString("yyyyMM");
						log.LogInformation($"Downloading Consumption Summary data from month: {lastMonth}");
						await DownloadAzureConsumptionSummaryLegacy(enrollment, lastMonth, token, log);
						log.LogInformation($"Downloading Marketplace Summary data from month: {lastMonth}");
						await DownloadAzureMarketplaceSummaryLegacy(enrollment, lastMonth, token, log);

						// Azure Consumption Summary and Azure Marketplace data from current month
						string currentMonth = currentDate.ToString("yyyyMM");
						log.LogInformation($"Downloading Consumption Summary data from month: {currentMonth}");
						await DownloadAzureConsumptionSummaryLegacy(enrollment, currentMonth, token, log);
						log.LogInformation($"Downloading Marketplace Summary data from month: {currentMonth}");
						await DownloadAzureMarketplaceSummaryLegacy(enrollment, currentMonth, token, log);


						// Azure cost details download
						log.LogInformation($"Generating data from: {currentDate}");
						string blobPath = "";
						try
						{
							blobPath = await GenerateAzureUsageDetailsLegacy(enrollment, token, currentDate.ToString("yyyy-MM-dd"),log);
							log.LogInformation($"Downloading data for: {currentDate}");

							string filename = DownloadAzureUsageDetailsFileLegacy(blobPath, enrollment, currentDate.ToString("yyyyMMdd"),log);
							log.LogInformation($"Data Downloaded for : {currentDate.ToString("yyyy-MM-dd")}");

							log.LogInformation($"Processing data from: {currentDate.ToString("yyyy-MM-dd")}");
							filename = ConvertAzureUsageDetailsFileLegacy(filename,log);
							log.LogInformation($"Processed data from: {currentDate.ToString("yyyy-MM-dd")}");

							log.LogInformation($"uploading data from: {currentDate.ToString("yyyy-MM-dd")}");
							await Common.UploadBlob("azure", filename,log);
							log.LogInformation($"Uploaded data from: {currentDate.ToString("yyyy-MM-dd")}");
							log.LogInformation($"Uploaded file: {filename}");

							log.LogInformation($"Finished with data from: {currentDate}"); log.LogInformation($"Data generated for: {currentDate}");
						}
						catch
						{
							log.LogWarning($"No data available for enrollment {enrollment} the day {currentDate.ToString("yyyy-MM-dd")}");

						}               
					}
				}
			}
			catch (Exception e)
			{
				log.LogDebug(e.ToString());
				throw;
			}
		}

	}
}
