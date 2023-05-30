using Azure.Security.KeyVault.Secrets;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInform
{
    internal class GCP
    {
		public static string GetBigqueryData(DateTime currentDate)
		{
			try
			{
				//ILogger Initialization
				ILoggerFactory _log;
				_log = new LoggerFactory();
				var log = _log.CreateLogger($"Starting Process of GPC Data for day: {currentDate}");

				string date = currentDate.ToString("yyyy-MM-dd");

				string fileDate = currentDate.ToString("yyyyMMdd");


				string filePath = "C:/home/data/";
				string fileName = "GCP-Usagedetails-" + fileDate + ".csv";
				string fullFile = filePath + fileName;

				log.LogInformation("Generating file " + fullFile);

				//Get BigQuery Paramenter from Environment Variables
				var GPCBigQueryProject = Environment.GetEnvironmentVariable("GPCBigQueryProject");
				var GPCBigQueryDatabase = Environment.GetEnvironmentVariable("GPCBigQueryDatabase");
				var GPCBigQueryTable = Environment.GetEnvironmentVariable("GPCBigQueryTable");

				//Retrieve connection json from keyvault
				log.LogInformation("Recovering from KeyVault secret: GPCsecret");
				KeyVaultSecret secret = Common.GetSecret("GPCsecret", log);

				//Connect to BigQuery Database
				var credential = GoogleCredential.FromJson(secret.Value);
				var client = BigQueryClient.Create(GPCBigQueryProject, credential);
				//var client = BigQueryClient.Create(GPCBigQueryProject);


				log.LogInformation("Connecting to BigQuery Database");
				var table = client.GetTable(GPCBigQueryProject, GPCBigQueryDatabase, GPCBigQueryTable);

				var query = $"SELECT billing_account_id," +
							$"service.id as serviceid," +
							$"service.description as servicedescription," +
							$"sku.id as sku_id," +
							$"sku.description as sku_description," +
							$"cast(date(usage_start_time) as date) as usage_start_time," +
							$"cast(date(usage_end_time) as date) as usage_end_time," +
							$"project.id as projectid," +
							$"project.number as projectnumber," +
							$"project.name as projectname," +
							$"project.ancestry_numbers as projec_ancestry_numbers," +
							$"location.location as location," +
							$"location.country as country," +
							$"location.region as region," +
							$"location.zone as zone," +
							$"export_time," +
							$"cost + IFNULL((SELECT sum(c.amount) FROM UNNEST(credits) c), 0) AS total," +
							$"currency," +
							$"currency_conversion_rate," +
							$"usage.amount as usage," +
							$"usage.unit as unit," +
							$"usage.amount_in_pricing_units as amount_in_pricing_units," +
							$"usage.pricing_unit as pricing_unit," +
							$"labels," +
							$"project.labels," +
							$"system_labels, " +
							$"invoice.month " +
							$"FROM {table}" +
							//$"WHERE usage_start_time = \"{date}\"";  //Estas linea solo es para pruebas!!!
							$"WHERE cast(date(usage_start_time) as date) = \"{date}\" and cost_type like 'regular'";

				log.LogInformation(query);
				log.LogInformation("Querying BigData Database");
				var results = client.ExecuteQuery(query, parameters: null);

				//StringBuilder sbRtn = new StringBuilder();
				log.LogInformation("Formating BigQuery results");
				var header = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#{9}#{10}#{11}#{12}#{13}#{14}#{15}#{16}#{17}#{18}#{19}#{20}#{21}#{22}#{23}#{24}#{25}#{26}\n",
										   "billing_account_id",
										   "serviceid",
										   "servicedescription",
										   "sku_id",
										   "sku_description",
										   "usage_start_time",
										   "usage_end_time",
										   "projectid",
										   "projectnumber",
										   "projectname",
										   "projec_ancestry_numbers",
										   "location",
										   "country",
										   "region",
										   "zone",
										   "export_time",
										   "cost",
										   "currency",
										   "currency_conversion_rate",
										   "usage",
										   "unit",
										   "amount_in_pricing_units",
										   "pricing_unit",
										   "labels",
										   "Project_labels",
										   "system_labels",
										   "billingmonthid"
										   );

				File.WriteAllText(fullFile, header);
				//sbRtn.AppendLine(header);
				var total = results.TotalRows;
				int i = 0;
				foreach (BigQueryRow row in results)
				{
					i++;
					if (i % 1000 == 0)
					{
						log.LogInformation("Processing line " + i + " from " + total);
					}
					string row00, row01, row02, row03, row04, row07, row08, row09, row10, row11, row12, row13, row14, row16, row17, row18, row19, row20, row21, row22, row23, row24, row25, row26;

					if (i % 1000 == 0) { Console.WriteLine("Procesando linea " + i); }
					if (!string.IsNullOrEmpty((string)row[0])) { row00 = ((string)row[0]).Replace("#", ""); } else { row00 = ""; }
					if (!string.IsNullOrEmpty((string)row[1])) { row01 = ((string)row[1]).Replace("#", ""); } else { row01 = ""; }
					if (!string.IsNullOrEmpty((string)row[2])) { row02 = ((string)row[2]).Replace("#", ""); } else { row02 = ""; }
					if (!string.IsNullOrEmpty((string)row[3])) { row03 = ((string)row[3]).Replace("#", ""); } else { row03 = ""; }
					if (!string.IsNullOrEmpty((string)row[4])) { row04 = ((string)row[4]).Replace("#", ""); } else { row04 = ""; }
					DateTime row05 = (DateTime)row[5];
					DateTime row06 = (DateTime)row[6];
					if (!string.IsNullOrEmpty((string)row[7])) { row07 = ((string)row[7]).Replace("#", ""); } else { row07 = ""; }
					if (!string.IsNullOrEmpty((string)row[8])) { row08 = ((string)row[8]).Replace("#", ""); } else { row08 = ""; }
					if (!string.IsNullOrEmpty((string)row[9])) { row09 = ((string)row[9]).Replace("#", ""); } else { row09 = ""; }
					if (!string.IsNullOrEmpty((string)row[10])) { row10 = ((string)row[10]).Replace("#", ""); } else { row10 = ""; }
					if (!string.IsNullOrEmpty((string)row[11])) { row11 = ((string)row[11]).Replace("#", ""); } else { row11 = ""; }
					if (!string.IsNullOrEmpty((string)row[12])) { row12 = ((string)row[12]).Replace("#", ""); } else { row12 = ""; }
					if (!string.IsNullOrEmpty((string)row[13])) { row13 = ((string)row[13]).Replace("#", ""); } else { row13 = ""; }
					if (!string.IsNullOrEmpty((string)row[14])) { row14 = ((string)row[14]).Replace("#", ""); } else { row14 = ""; }
					DateTime row15 = (DateTime)row[15];
					if (!string.IsNullOrEmpty(Convert.ToString(row[16]))) { row16 = (Convert.ToString(row[16])); } else { row16 = ""; }
					if (!string.IsNullOrEmpty((string)row[17])) { row17 = ((string)row[17]).Replace("#", ""); } else { row17 = ""; }
					if (!string.IsNullOrEmpty(Convert.ToString(row[18]))) { row18 = (Convert.ToString(row[18])); } else { row18 = ""; }
					if (!string.IsNullOrEmpty(Convert.ToString(row[19]))) { row19 = (Convert.ToString(row[19])); } else { row19 = ""; }
					if (!string.IsNullOrEmpty((string)row[20])) { row20 = ((string)row[20]).Replace("#", ""); } else { row20 = ""; }
					if (!string.IsNullOrEmpty(Convert.ToString(row[21]))) { row21 = (Convert.ToString(row[21])); } else { row21 = ""; }
					if (!string.IsNullOrEmpty((string)row[22])) { row22 = ((string)row[22]).Replace("#", ""); } else { row22 = ""; }
					row23 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[23]);
					row24 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[24]);
					row25 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[25]);
                    if (!string.IsNullOrEmpty((string)row[26])) { row26 = ((string)row[26]).Replace("#", ""); } else { row26 = ""; }
                    var listResults = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#{9}#{10}#{11}#{12}#{13}#{14}#{15}#{16}#{17}#{18}#{19}#{20}#{21}#{22}#{23}#{24}#{25}#{26}\n",
						row00,
						row01,
						row02,
						row03,
						row04,
						row05,
						row06,
						row07,
						row08,
						row09,
						row10,
						row11,
						row12,
						row13,
						row14,
						row15,
						row16,
						row17,
						row18,
						row19,
						row20,
						row21,
						row22,
						row23,
						row24,
						row25,
						row26
						);
					//sbRtn.AppendLine(listResults);
					File.AppendAllText(fullFile, listResults);
				}

				return (fileName);
			}
			catch
			{
				throw;
			}
		}
        
		public static string GetBigqueryDataRefunds(DateTime endDate,ILogger log)
        {
            try
            {
				
                //ILogger Initialization
				log.LogInformation($"Starting Process of GPC Data for day: {endDate}");


                string startDate = endDate.AddDays(-180).ToString("yyyy-MM-dd");
				string endDateStr = endDate.ToString("yyyy-MM-dd");
                string fileDate = endDate.ToString("yyyyMMdd");



                string filePath = "C:/home/data/";
                string fileName = "GCP-UsagedetailsRefunds-" + fileDate + ".csv";
                string fullFile = filePath + fileName;

                log.LogInformation("Generating file " + fullFile);

                //Get BigQuery Paramenter from Environment Variables
                var GPCBigQueryProject = Environment.GetEnvironmentVariable("GPCBigQueryProject");
                var GPCBigQueryDatabase = Environment.GetEnvironmentVariable("GPCBigQueryDatabase");
                var GPCBigQueryTable = Environment.GetEnvironmentVariable("GPCBigQueryTable");

                //Retrieve connection json from keyvault
                log.LogInformation("Recovering from KeyVault secret: GPCsecret");
                KeyVaultSecret secret = Common.GetSecret("GPCsecret", log);

                //Connect to BigQuery Database
                var credential = GoogleCredential.FromJson(secret.Value);
                var client = BigQueryClient.Create(GPCBigQueryProject, credential);
                //var client = BigQueryClient.Create(GPCBigQueryProject);


                log.LogInformation("Connecting to BigQuery Database");
                var table = client.GetTable(GPCBigQueryProject, GPCBigQueryDatabase, GPCBigQueryTable);

                var query = $"SELECT billing_account_id," +
                            $"service.id as serviceid," +
                            $"service.description as servicedescription," +
                            $"sku.id as sku_id," +
                            $"sku.description as sku_description," +
                            $"cast(date(usage_start_time) as date) as usage_start_time," +
                            $"cast(date(usage_end_time) as date) as usage_end_time," +
                            $"project.id as projectid," +
                            $"project.number as projectnumber," +
                            $"project.name as projectname," +
                            $"project.ancestry_numbers as projec_ancestry_numbers," +
                            $"location.location as location," +
                            $"location.country as country," +
                            $"location.region as region," +
                            $"location.zone as zone," +
                            $"export_time," +
                            $"cost," +
                            $"currency," +
                            $"currency_conversion_rate," +
                            $"usage.amount as usage," +
                            $"usage.unit as unit," +
                            $"usage.amount_in_pricing_units as amount_in_pricing_units," +
                            $"usage.pricing_unit as pricing_unit," +
                            $"labels," +
                            $"project.labels," +
                            $"system_labels " +
                            $"FROM {table}" +
                            $"WHERE cast(date(usage_start_time) as date) >= \"{startDate}\" and cast(date(usage_start_time) as date) <= \"{endDateStr}\" and cost<0";

                log.LogInformation(query);
                log.LogInformation("Querying BigData Database");
                var results = client.ExecuteQuery(query, parameters: null);

                //StringBuilder sbRtn = new StringBuilder();
                log.LogInformation("Formating BigQuery results");
                var header = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#{9}#{10}#{11}#{12}#{13}#{14}#{15}#{16}#{17}#{18}#{19}#{20}#{21}#{22}#{23}#{24}#{25}\n",
                                           "billing_account_id",
                                           "serviceid",
                                           "servicedescription",
                                           "sku_id",
                                           "sku_description",
                                           "usage_start_time",
                                           "usage_end_time",
                                           "projectid",
                                           "projectnumber",
                                           "projectname",
                                           "projec_ancestry_numbers",
                                           "location",
                                           "country",
                                           "region",
                                           "zone",
                                           "export_time",
                                           "cost",
                                           "currency",
                                           "currency_conversion_rate",
                                           "usage",
                                           "unit",
                                           "amount_in_pricing_units",
                                           "pricing_unit",
                                           "labels",
                                           "Project_labels",
                                           "system_labels");

                File.WriteAllText(fullFile, header);
                //sbRtn.AppendLine(header);
                var total = results.TotalRows;
                int i = 0;
                foreach (BigQueryRow row in results)
                {
                    i++;
                    if (i % 1000 == 0)
                    {
                        log.LogInformation("Processing line " + i + " from " + total);
                    }
                    string row00, row01, row02, row03, row04, row07, row08, row09, row10, row11, row12, row13, row14, row16, row17, row18, row19, row20, row21, row22, row23, row24, row25;

                    if (i % 1000 == 0) { Console.WriteLine("Procesando linea " + i); }
                    if (!string.IsNullOrEmpty((string)row[0])) { row00 = ((string)row[0]).Replace("#", ""); } else { row00 = ""; }
                    if (!string.IsNullOrEmpty((string)row[1])) { row01 = ((string)row[1]).Replace("#", ""); } else { row01 = ""; }
                    if (!string.IsNullOrEmpty((string)row[2])) { row02 = ((string)row[2]).Replace("#", ""); } else { row02 = ""; }
                    if (!string.IsNullOrEmpty((string)row[3])) { row03 = ((string)row[3]).Replace("#", ""); } else { row03 = ""; }
                    if (!string.IsNullOrEmpty((string)row[4])) { row04 = ((string)row[4]).Replace("#", ""); } else { row04 = ""; }
                    DateTime row05 = (DateTime)row[5];
                    DateTime row06 = (DateTime)row[6];
                    if (!string.IsNullOrEmpty((string)row[7])) { row07 = ((string)row[7]).Replace("#", ""); } else { row07 = ""; }
                    if (!string.IsNullOrEmpty((string)row[8])) { row08 = ((string)row[8]).Replace("#", ""); } else { row08 = ""; }
                    if (!string.IsNullOrEmpty((string)row[9])) { row09 = ((string)row[9]).Replace("#", ""); } else { row09 = ""; }
                    if (!string.IsNullOrEmpty((string)row[10])) { row10 = ((string)row[10]).Replace("#", ""); } else { row10 = ""; }
                    if (!string.IsNullOrEmpty((string)row[11])) { row11 = ((string)row[11]).Replace("#", ""); } else { row11 = ""; }
                    if (!string.IsNullOrEmpty((string)row[12])) { row12 = ((string)row[12]).Replace("#", ""); } else { row12 = ""; }
                    if (!string.IsNullOrEmpty((string)row[13])) { row13 = ((string)row[13]).Replace("#", ""); } else { row13 = ""; }
                    if (!string.IsNullOrEmpty((string)row[14])) { row14 = ((string)row[14]).Replace("#", ""); } else { row14 = ""; }
                    DateTime row15 = (DateTime)row[15];
                    if (!string.IsNullOrEmpty(Convert.ToString(row[16]))) { row16 = (Convert.ToString(row[16])); } else { row16 = ""; }
                    if (!string.IsNullOrEmpty((string)row[17])) { row17 = ((string)row[17]).Replace("#", ""); } else { row17 = ""; }
                    if (!string.IsNullOrEmpty(Convert.ToString(row[18]))) { row18 = (Convert.ToString(row[18])); } else { row18 = ""; }
                    if (!string.IsNullOrEmpty(Convert.ToString(row[19]))) { row19 = (Convert.ToString(row[19])); } else { row19 = ""; }
                    if (!string.IsNullOrEmpty((string)row[20])) { row20 = ((string)row[20]).Replace("#", ""); } else { row20 = ""; }
                    if (!string.IsNullOrEmpty(Convert.ToString(row[21]))) { row21 = (Convert.ToString(row[21])); } else { row21 = ""; }
                    if (!string.IsNullOrEmpty((string)row[22])) { row22 = ((string)row[22]).Replace("#", ""); } else { row22 = ""; }
                    row23 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[23]);
                    row24 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[24]);
                    row25 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[25]);
                    var listResults = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#{9}#{10}#{11}#{12}#{13}#{14}#{15}#{16}#{17}#{18}#{19}#{20}#{21}#{22}#{23}#{24}#{25}\n",
                        row00,
                        row01,
                        row02,
                        row03,
                        row04,
                        row05,
                        row06,
                        row07,
                        row08,
                        row09,
                        row10,
                        row11,
                        row12,
                        row13,
                        row14,
                        row15,
                        row16,
                        row17,
                        row18,
                        row19,
                        row20,
                        row21,
                        row22,
                        row23,
                        row24,
                        row25
                        );
                    //sbRtn.AppendLine(listResults);
                    File.AppendAllText(fullFile, listResults);
                }

                return (fileName);
            }
            catch (Exception e)
            {
				log.LogError(e.Message);
                throw;
            }
        }

        public static string GetBigqueryDataCredits(DateTime currentDate)
        {
            try
            {
                //ILogger Initialization
                ILoggerFactory _log;
                _log = new LoggerFactory();
                var log = _log.CreateLogger($"Starting Process of GPC Credits Data for day: {currentDate}");

                string date = currentDate.ToString("yyyy-MM-dd");

                string fileDate = currentDate.ToString("yyyyMMdd");


                string filePath = "C:/home/data/";
                string fileName = "GCP-CreditsDetails-" + fileDate + ".csv";
                string fullFile = filePath + fileName;

                log.LogInformation("Generating file " + fullFile);

                //Get BigQuery Paramenter from Environment Variables
                var GPCBigQueryProject = Environment.GetEnvironmentVariable("GPCBigQueryProject");
                var GPCBigQueryDatabase = Environment.GetEnvironmentVariable("GPCBigQueryDatabase");
                var GPCBigQueryTable = Environment.GetEnvironmentVariable("GPCBigQueryTable");

                //Retrieve connection json from keyvault
                log.LogInformation("Recovering from KeyVault secret: GPCsecret");
                KeyVaultSecret secret = Common.GetSecret("GPCsecret", log);

                //Connect to BigQuery Database
                var credential = GoogleCredential.FromJson(secret.Value);
                var client = BigQueryClient.Create(GPCBigQueryProject, credential);
                //var client = BigQueryClient.Create(GPCBigQueryProject);


                log.LogInformation("Connecting to BigQuery Database");
                var table = client.GetTable(GPCBigQueryProject, GPCBigQueryDatabase, GPCBigQueryTable);

                var query = $"SELECT billing_account_id," +
                            $"service.id as serviceid," +
                            $"concat(service.description, \" - \",c.name, \" - \",c.type) as servicedescription," +
                            $"sku.id as sku_id," +
                            $"sku.description as sku_description," +
                            $"cast(date(usage_start_time) as date) as usage_start_time," +
                            $"cast(date(usage_end_time) as date) as usage_end_time," +
                            $"project.id as projectid," +
                            $"project.number as projectnumber," +
                            $"project.name as projectname," +
                            $"project.ancestry_numbers as projec_ancestry_numbers," +
                            $"location.location as location," +
                            $"location.country as country," +
                            $"location.region as region," +
                            $"location.zone as zone," +
                            $"export_time," +
                            $"c.amount as cost," +
                            $"currency," +
                            $"currency_conversion_rate," +
                            $"usage.amount as usage," +
                            $"usage.unit as unit," +
                            $"usage.amount_in_pricing_units as amount_in_pricing_units," +
                            $"usage.pricing_unit as pricing_unit," +
                            $"labels," +
                            $"project.labels," +
                            $"system_labels " +
                            $"FROM {table},UNNEST(credits) as c " +
                            //$"WHERE usage_start_time = \"{date}\"";  //Estas linea solo es para pruebas!!!
                            $"WHERE cast(date(usage_start_time) as date) = \"{date}\"";

                log.LogInformation(query);
                log.LogInformation("Querying BigData Database");
                var results = client.ExecuteQuery(query, parameters: null);

                //StringBuilder sbRtn = new StringBuilder();
                log.LogInformation("Formating BigQuery results");
                var header = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#{9}#{10}#{11}#{12}#{13}#{14}#{15}#{16}#{17}#{18}#{19}#{20}#{21}#{22}#{23}#{24}#{25}\n",
                                           "billing_account_id",
                                           "serviceid",
                                           "servicedescription",
                                           "sku_id",
                                           "sku_description",
                                           "usage_start_time",
                                           "usage_end_time",
                                           "projectid",
                                           "projectnumber",
                                           "projectname",
                                           "projec_ancestry_numbers",
                                           "location",
                                           "country",
                                           "region",
                                           "zone",
                                           "export_time",
                                           "cost",
                                           "currency",
                                           "currency_conversion_rate",
                                           "usage",
                                           "unit",
                                           "amount_in_pricing_units",
                                           "pricing_unit",
                                           "labels",
                                           "Project_labels",
                                           "system_labels");

                File.WriteAllText(fullFile, header);
                //sbRtn.AppendLine(header);
                var total = results.TotalRows;
                int i = 0;
                foreach (BigQueryRow row in results)
                {
                    i++;
                    if (i % 1000 == 0)
                    {
                        log.LogInformation("Processing line " + i + " from " + total);
                    }
                    string row00, row01, row02, row03, row04, row07, row08, row09, row10, row11, row12, row13, row14, row16, row17, row18, row19, row20, row21, row22, row23, row24, row25;

                    if (i % 1000 == 0) { Console.WriteLine("Procesando linea " + i); }
                    if (!string.IsNullOrEmpty((string)row[0])) { row00 = ((string)row[0]).Replace("#", ""); } else { row00 = ""; }
                    if (!string.IsNullOrEmpty((string)row[1])) { row01 = ((string)row[1]).Replace("#", ""); } else { row01 = ""; }
                    if (!string.IsNullOrEmpty((string)row[2])) { row02 = ((string)row[2]).Replace("#", ""); } else { row02 = ""; }
                    if (!string.IsNullOrEmpty((string)row[3])) { row03 = ((string)row[3]).Replace("#", ""); } else { row03 = ""; }
                    if (!string.IsNullOrEmpty((string)row[4])) { row04 = ((string)row[4]).Replace("#", ""); } else { row04 = ""; }
                    DateTime row05 = (DateTime)row[5];
                    DateTime row06 = (DateTime)row[6];
                    if (!string.IsNullOrEmpty((string)row[7])) { row07 = ((string)row[7]).Replace("#", ""); } else { row07 = ""; }
                    if (!string.IsNullOrEmpty((string)row[8])) { row08 = ((string)row[8]).Replace("#", ""); } else { row08 = ""; }
                    if (!string.IsNullOrEmpty((string)row[9])) { row09 = ((string)row[9]).Replace("#", ""); } else { row09 = ""; }
                    if (!string.IsNullOrEmpty((string)row[10])) { row10 = ((string)row[10]).Replace("#", ""); } else { row10 = ""; }
                    if (!string.IsNullOrEmpty((string)row[11])) { row11 = ((string)row[11]).Replace("#", ""); } else { row11 = ""; }
                    if (!string.IsNullOrEmpty((string)row[12])) { row12 = ((string)row[12]).Replace("#", ""); } else { row12 = ""; }
                    if (!string.IsNullOrEmpty((string)row[13])) { row13 = ((string)row[13]).Replace("#", ""); } else { row13 = ""; }
                    if (!string.IsNullOrEmpty((string)row[14])) { row14 = ((string)row[14]).Replace("#", ""); } else { row14 = ""; }
                    DateTime row15 = (DateTime)row[15];
                    if (!string.IsNullOrEmpty(Convert.ToString(row[16]))) { row16 = (Convert.ToString(row[16])); } else { row16 = ""; }
                    if (!string.IsNullOrEmpty((string)row[17])) { row17 = ((string)row[17]).Replace("#", ""); } else { row17 = ""; }
                    if (!string.IsNullOrEmpty(Convert.ToString(row[18]))) { row18 = (Convert.ToString(row[18])); } else { row18 = ""; }
                    if (!string.IsNullOrEmpty(Convert.ToString(row[19]))) { row19 = (Convert.ToString(row[19])); } else { row19 = ""; }
                    if (!string.IsNullOrEmpty((string)row[20])) { row20 = ((string)row[20]).Replace("#", ""); } else { row20 = ""; }
                    if (!string.IsNullOrEmpty(Convert.ToString(row[21]))) { row21 = (Convert.ToString(row[21])); } else { row21 = ""; }
                    if (!string.IsNullOrEmpty((string)row[22])) { row22 = ((string)row[22]).Replace("#", ""); } else { row22 = ""; }
                    row23 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[23]);
                    row24 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[24]);
                    row25 = JsonConvert.SerializeObject((Dictionary<string, object>[])row[25]);
                    var listResults = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}#{9}#{10}#{11}#{12}#{13}#{14}#{15}#{16}#{17}#{18}#{19}#{20}#{21}#{22}#{23}#{24}#{25}\n",
                        row00,
                        row01,
                        row02,
                        row03,
                        row04,
                        row05,
                        row06,
                        row07,
                        row08,
                        row09,
                        row10,
                        row11,
                        row12,
                        row13,
                        row14,
                        row15,
                        row16,
                        row17,
                        row18,
                        row19,
                        row20,
                        row21,
                        row22,
                        row23,
                        row24,
                        row25
                        );
                    //sbRtn.AppendLine(listResults);
                    File.AppendAllText(fullFile, listResults);
                }

                return (fileName);
            }
            catch
            {
                throw;
            }
        }

        /*
        public static async Task GetGCPData(DateTime startDate, DateTime endDate, ILogger log)
		{
			try
			{
				log.LogInformation("Starting Function - Recovering GCP UsageDetails Data");
				for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
				{
					log.LogInformation("Querying BigQuery for day " + currentDate.ToString("yyyy-MM-dd"));
					string fileName = GCP.GetBigqueryData(currentDate);

					log.LogInformation("BigQuery data processed.");

					log.LogInformation("Uploading gpc data from day: " + currentDate.ToString("yyyy-MM-dd"));
					await Common.UploadBlob("gcp", fileName,log);
					log.LogInformation($"GCP data from day: {currentDate.ToString("yyyy-MM-dd")} uploaded");
				}
			}
			catch (Exception e)
			{ 
				log.LogError (e.ToString());
				throw;
			}

		}

		
		public static int GCPTempUsageDetails(DateTime startDate, DateTime endDate, ILogger log)
		{
			try
			{
				log.LogInformation("Creating Temporary table");
				string queryCreateTable = "BEGIN TRY " +
										$"DROP TABLE [TEMP_GCP_Usagedetails] " +
										$"END TRY " +
										$"BEGIN CATCH " +
										$"END CATCH; " +
										$"CREATE TABLE [dbo].[TEMP_GCP_Usagedetails](" +
										$"[billing_account_id] [nvarchar](max) NULL," +
										$"[serviceid] [nvarchar](max) NULL," +
										$"[servicedescription] [nvarchar](max) NULL," +
										$"[sku_id] [nvarchar](max) NULL," +
										$"[sku_description] [nvarchar](max) NULL," +
										$"[usage_start_time] [datetime2](7) NULL," +
										$"[usage_end_time] [datetime2](7) NULL," +
										$"[projectid] [nvarchar](max) NULL," +
										$"[projectnumber] [nvarchar](max) NULL," +
										$"[projectname] [nvarchar](max) NULL," +
										$"[project_ancestry_numbers] [nvarchar](max) NULL," +
										$"[location] [nvarchar](max) NULL," +
										$"[country] [nvarchar](max) NULL," +
										$"[region] [nvarchar](max) NULL," +
										$"[zone] [nvarchar](max) NULL," +
										$"[export_time] [datetime2](7) NULL," +
										$"[cost] [nvarchar](max) NULL," +
										$"[currency] [nvarchar](max) NULL," +
										$"[currency_conversion_rate] [float] NULL," +
										$"[usage] [nvarchar](max) NULL," +
										$"[unit] [nvarchar](max) NULL," +
										$"[amount_in_pricing_units] [nvarchar](max) NULL," +
										$"[pricing_unit] [nvarchar](max) NULL," +
										$"[labels] [nvarchar](max) NULL," +
										$"[Project_labels] [nvarchar](max) NULL," +
										$"[system_labels] [nvarchar](max) NULL" +
									$")";

				Common.SqlInsert(queryCreateTable, log);


				for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
				{
					string fileName = "GCP-Usagedetails-" + currentDate.ToString("yyyyMMdd") + ".csv";
					log.LogInformation("Inserting data into TEMP Table from file " + fileName);
					string queryBulkInsertTempTable = "BULK INSERT TEMP_GCP_UsageDetails FROM '" + fileName + "' " +
													$"WITH(" +
														$"CHECK_CONSTRAINTS, " +
														$"DATA_SOURCE = 'GCPUsageDetailsDataLake', " +
														$"DATAFILETYPE = 'char', " +
														$"FIELDTERMINATOR = '#', " +
														$"ROWTERMINATOR = '0x0a', " +
														$"FIRSTROW = 2, " +
														$"KEEPIDENTITY, " +
														$"TABLOCK " +
													$")";

					Common.SqlInsert(queryBulkInsertTempTable, log);
				}
				return (0);
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
				throw;
			}
		}

		public static int GCPPopulateSQLData(DateTime startDate, DateTime endDate, ILogger log)
		{
			try
			{
				log.LogInformation("Inserting data into UsageDetail Table");

				string queryInsertUsageDetails = "insert into usagedetail " +
													$"select " +
														$"CONCAT(YEAR(usage_start_time),format(usage_start_time,'MM'),format(usage_start_time,'dd')) AS BillingDayId, " +
														$"billing_account_id as EnrollmentId, " +
														$"usage_start_time as date, " +
														$"YEAR(usage_start_time) as year, " +
														$"format(usage_start_time,'MM') as Month, " +
														$"format(usage_start_time,'dd') as Day, " +
														$"'' as Departmentname, " +
														$"'' as AccountOwnerId, " +
														$"'' as AccountName, " +
														$"projectnumber as SubscriptionGuid, " +
														$"ProjectName as SubscriptionName, " +
														$"'' as OfferId, " +
														$"ProjectName as resourceGroup, " +
														$"region as ResourceLocation, " +
														$"sku_id as PartNumber, " +
														$"sku_description as Product, " +
														$"'' as resourcename, " +
														$"usage as ConsumedQuantity, " +
														$"unit as UnitOfMeasure, " +
														$"amount_in_pricing_units as ResourceRate, " +
														$"cost as Cost, " +
														$"servicedescription as ConsumedService, " +
														$"servicedescription as ServiceName,  " +
														$"sku_description as ServiceTier, " +
														$"'' as MeterId_ResourceGuid, " +
														$"servicedescription as MeterName, " +
														$"servicedescription as MeterCategory, " +
														$"sku_description as MeterSubCategory, " +
														@"concat('{',replace(replace(replace(replace(replace(replace(replace(replace(concat(labels,system_labels,Project_labels),'{""key"":',''),',""value""',''),'[',''),']',''),'}',''),'""""','"",""'),':"",""',':""""'),'"":""""""','"":"""",""'),'}') as Tags, " +
														$"'' as InstanceId, " +
														$"'' as CostCenter, " +
														$"'' as ChargesBilledSeparately, " +
														$"'' as GovernanceTags, " +
														$"'' as DSPtags, " +
														$"servicedescription as Category, " +
														@"concat('{',replace(replace(replace(replace(replace(replace(replace(replace(concat(labels,system_labels,Project_labels),'{""key"":',''),',""value""',''),'[',''),']',''),'}',''),'""""','"",""'),':"",""',':""""'),'"":""""""','"":"""",""'),'}') as FullTag, " +
														$"'GCP' as Cloud, " +
														$"CONCAT(YEAR(usage_start_time),format(usage_start_time,'MM')) asBillingMonthID  " +
													$"from ( " +
														$"SELECT [billing_account_id] " +
															$",[serviceid] " +
															$",[servicedescription] " +
															$",[sku_id] " +
															$",[sku_description] " +
															$",CONVERT (DATE, usage_start_time) as usage_start_time " +
															$",[projectid] " +
															$",[projectnumber] " +
															$",[projectname] " +
															$",[project_ancestry_numbers] " +
															$",[location] " +
															$",[country] " +
															$",[region] " +
															$",[zone] " +
															$",sum(convert(float,[cost])) as cost " +
															$",[currency] " +
															$",[currency_conversion_rate] " +
															$",sum(convert(float,[usage])) as usage " +
															$",[unit] " +
															$",sum(convert(float,[amount_in_pricing_units])) amount_in_pricing_units " +
															$",[pricing_unit] " +
															$",[labels] " +
															$",[Project_labels] " +
															$",[system_labels] " +
														  $"FROM [dbo].[TEMP_GCP_Usagedetails] " +
														  $"WHERE " +
															$" CAST(usage_start_time as date) >= '" + startDate.ToString("yyyy-MM-dd") + "'" +
															$" and CAST(usage_start_time as date) <= '" + endDate.ToString("yyyy-MM-dd") + "'" +
														$" GROUP BY " +
															$"CONVERT (DATE, usage_start_time), " +
															$"billing_account_id, " +
															$"serviceid, " +
															$"servicedescription, " +
															$"sku_id, " +
															$"sku_description, " +
															$"projectid, " +
															$"projectnumber, " +
															$"projectname, " +
															$"project_ancestry_numbers, " +
															$"location, " +
															$"country, " +
															$"region, " +
															$"zone, " +
															$"currency, " +
															$"currency_conversion_rate, " +
															$"unit, " +
															$"pricing_unit, " +
															$"labels, " +
															$"Project_labels, " +
															$"system_labels " +
														$") AS T1 ";

				//log.LogInformation(queryInsertUsageDetails);  For debugging purpouses

				Common.SqlInsert(queryInsertUsageDetails, log);
				log.LogInformation("Data inserted in UsageDetail table");
				return (0);
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
				throw;
			}
		}

		public static int GPCPopulateConsumptionSummary(DateTime startDate, DateTime endDate, ILogger log)  //This function is not ready to work with more than one GCP Account
		{
			try
			{
				log.LogInformation("Inserting GPC data into ConsumptionSummary Table");

				string startBillingMonthID = startDate.ToString("yyyyMM");
				string endBillingMonthID = endDate.ToString("yyyyMM");

				if (!string.Equals(startBillingMonthID, endBillingMonthID))  //If there is data from two Months, Update the data of the older
				{
					log.LogInformation("Inserting data in ConsumptionSummary table for month " + startBillingMonthID);
					string queryInsertLastMonth = $" begin try" +
											$" delete from ConsumptionSummary where BillingPeriod=" + startBillingMonthID +
											$" End try " +
												$" begin catch " +
												$" end catch " +
												$" insert into ConsumptionSummary " +
												$" select " +
													$" EnrollmentId as enrollment, " +
													$" BillingMonthID as billingperiod, " +
													$" SUBSTRING(cast(BillingMonthID as nvarchar(20)),1,4) + '-' + SUBSTRING(cast(BillingMonthID as nvarchar(20)),5,2)+'-01' as date, " +
													$" 'EUR' as CurrencyCode, " +
													$" '0' as beginingbalance, " +
													$" '0' as endingbalance, " +
													$" '0' as newpurchases, " +
													$" '0' as adjustements, " +
													$" '0' as utilized, " +
													$" '0' as serviceoverage, " +
													$" '0' as chargesbilledseparately, " +
													$" '0' as totaloverage, " +
													$" SUM(cost) as totalUsage, " +
													$" '0' as azuremarketplaceservicecharges, " +
													$" '' as newpurchasesdetails, " +
													$" '' as adjustmentsdetails, " +
													$" 'GCP' as cloud " +
												$" from usagedetail " +
												$" where " +
												$" BillingMonthID=" + startBillingMonthID +
												$" and enrollmentid like(select top(1) billing_account_id from TEMP_GCP_Usagedetails) " +
											$"  group by  " +
													$" EnrollmentId, " +
													$" BillingMonthID";

					Common.SqlInsert(queryInsertLastMonth, log);

					log.LogInformation("Data inserted in ConsumptionSummary table for month " + startBillingMonthID);

				}
				// Anyway, I insert the data of the newer
				log.LogInformation("Inserting data in ConsumptionSummary table for month " + endBillingMonthID);
				string queryInsertCurrentMonth = $" begin try" +
											$" delete from ConsumptionSummary where BillingPeriod=" + endBillingMonthID +
											$" End try " +
												$" begin catch " +
												$" end catch " +
												$" insert into ConsumptionSummary " +
												$" select " +
													$" EnrollmentId as enrollment, " +
													$" BillingMonthID as billingperiod, " +
													$" SUBSTRING(cast(BillingMonthID as nvarchar(20)),1,4) + '-' + SUBSTRING(cast(BillingMonthID as nvarchar(20)),5,2)+'-01' as date, " +
													$" 'EUR' as CurrencyCode, " +
													$" '0' as beginingbalance, " +
													$" '0' as endingbalance, " +
													$" '0' as newpurchases, " +
													$" '0' as adjustements, " +
													$" '0' as utilized, " +
													$" '0' as serviceoverage, " +
													$" '0' as chargesbilledseparately, " +
													$" '0' as totaloverage, " +
													$" SUM(cost) as totalUsage, " +
													$" '0' as azuremarketplaceservicecharges, " +
													$" '' as newpurchasesdetails, " +
													$" '' as adjustmentsdetails, " +
													$" 'GCP' as cloud " +
												$" from usagedetail " +
												$" where " +
												$" BillingMonthID=" + endBillingMonthID +
												$" and enrollmentid like(select top(1) billing_account_id from TEMP_GCP_Usagedetails) " +
											$"  group by  " +
													$" EnrollmentId, " +
													$" BillingMonthID";

				Common.SqlInsert(queryInsertCurrentMonth, log);

				log.LogInformation("Data inserted in ConsumptionSummary table for month " + endBillingMonthID);

				return (0);
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
				throw;
			}
		}
		*/
    }
}
