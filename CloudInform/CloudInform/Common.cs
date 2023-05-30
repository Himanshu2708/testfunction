using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudInform
{
    static class Common
    {
		public static KeyVaultSecret GetSecret(string key, ILogger log)
		{
			try
			{
				log.LogInformation($"Recovering secret: {key}");

				var keyVault = Environment.GetEnvironmentVariable("KEYVAULT");
				string keyVaultURI = "https://" + keyVault + ".vault.azure.net";

				var clientKey = new SecretClient(vaultUri: new Uri(keyVaultURI), credential: new DefaultAzureCredential());

				// Retrieve a secret using the secret client.
				KeyVaultSecret secret = clientKey.GetSecret(key);
				log.LogInformation("Secret recovered");
				return (secret);
			}
			catch 			{
				log.LogError($"Error recovering key {key}, verify that it exists on the KeyVault");
				throw ;
			}
		}


		public static async Task<IActionResult> UploadBlob(string cloud, string fileName, ILogger log)
		{
			try
			{
				log.LogInformation($"Starting file {fileName} upload to container {cloud}");

				//Startinng the code
				string filePath = "C:/home/data/";

				string fullFile = filePath + fileName;

				//string content = File.ReadAllText(fullFile);

				string storageAccount = Environment.GetEnvironmentVariable("DataLakeName");
				string container = cloud;
				var uri = "https://" + storageAccount + ".blob.core.windows.net/" + container;

				var client1 = new BlobContainerClient(new Uri(uri), new DefaultAzureCredential());
				log.LogInformation("connected to DataLake.");
				// Get content stream
				//using var stream = new MemoryStream(Encoding.ASCII.GetBytes(content.ToString()));

				// Upload blob
				log.LogInformation($"Uploading {fileName} to DataLake");

				try
				{
					client1.DeleteBlobIfExists(fileName);
				}
				catch (Exception e)
				{
					log.LogWarning(e.Message);
				}
				try
				{
					using (var fileStream = System.IO.File.OpenRead(@fullFile))
					{
						await client1.UploadBlobAsync(fileName, fileStream);
					}
					//await client1.UploadBlobAsync(fileName, stream);
				}
				catch (Exception e)
				{
					log.LogError(e.Message);
				}
				log.LogInformation(fileName + " uploaded to DataLake");

				File.Delete(fullFile);

				return new OkObjectResult("Blob uploaded");
			}
			catch
			{
				log.LogError($"Error uploading file {fileName}. Verify that the container exists and that the function has the proper permissions over the Storage Account");
				throw;
			}

		}

		public static string GetBearer(string tenant, string appId, string password, string resource,ILogger log)
		{
			try
			{
				if (resource != "https://manage.office.com/" && resource != "https://management.azure.com/")
				{
					log.LogError("Error in parameter resource. Valid values are 'https://manage.office.com/' and 'https://management.azure.com/'");
					throw new Exception();
				}
				log.LogDebug("Recovering Azure Bearer Token");

				var nvc = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("grant_type", "client_credentials"),
					new KeyValuePair<string, string>("client_id", appId),
					new KeyValuePair<string, string>("client_secret", password),
					new KeyValuePair<string, string>("resource", resource)
				};

				var url = $"https://login.microsoftonline.com/{tenant}/oauth2/token";

				using var client = new HttpClient();
				var req = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = new FormUrlEncodedContent(nvc)
				};

				using var res = client.SendAsync(req).Result;
				var jsonString = res.Content.ReadAsStringAsync().Result;
				var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
				log.LogDebug("Azure Bearer Token Recovered");
				return json.access_token;
			}
			catch(Exception e)
			{
				log.LogError("Line 69");
				log.LogError(e.Message.ToString());
				throw;
			}
		}

		public static async Task<String> GetAPIDataGet(string accessToken, string webApiUrl, ILogger log)
		{
			try
			{
				log.LogInformation("Getting data from API...");

				bool throttling = false;
				var httpClient = new HttpClient();
				HttpResponseMessage response;
				var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
				defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				do
				{
					response = await httpClient.GetAsync(webApiUrl);
					log.LogDebug($"HTTP Statuscode: { response.StatusCode}");
					switch ((int)response.StatusCode)
					{
						case 429:
							log.LogWarning("Throttling Exception. Waiting 5 seconds");
							throttling = true;
							Thread.Sleep(5000);
							break;

						default:
							throttling = false;
							break;
					}
				}
				while (throttling);
				string result = await response.Content.ReadAsStringAsync();
				return (result);
			}
			catch
			{
				log.LogError($"Error querying {webApiUrl}");
				throw;
			}
		}

		public static async Task<String> GetAPIDataPost(string accessToken, string webApiUrl, ILogger log)
		{
			try
			{
				log.LogInformation("Getting data from API...");

				bool throttling = false;
				var httpClient = new HttpClient();
				HttpResponseMessage response;
				HttpContent httpContent = new StringContent(string.Empty);

				var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
				defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				do
				{
					response = await httpClient.PostAsync(webApiUrl, httpContent);
					log.LogDebug($"HTTP Statuscode: {response.StatusCode}");
					switch ((int)response.StatusCode)
					{
						case 429:
							log.LogWarning("Throttling Exception. Waiting 5 seconds");
							throttling = true;
							Thread.Sleep(5000);
							break;

						default:
							throttling = false;
							break;
					}
				}
				while (throttling);
				string result = await response.Content.ReadAsStringAsync();
				return (result);
			}
			catch
			{
				log.LogError($"Error querying {webApiUrl}");
				throw;
			}

		}
		public static async Task<String> GetM365Token(ILogger log)
		{
			try
			{
				log.LogInformation("Recovering M365 Token");


				//This should be parametrized


				string appId = GetSecret("ApplicationID", log).Value;
				string secret = GetSecret("ApplicationSecret", log).Value;
				string tenantId = GetSecret("TenantID", log).Value;

				//Those are static URL and shuould remain here.
				string instance = "https://login.microsoftonline.com";
				string apiUrl = "https://graph.microsoft.com/";

				string authority = String.Format(CultureInfo.InvariantCulture, instance, tenantId);

				IConfidentialClientApplication app;
				app = ConfidentialClientApplicationBuilder.Create(appId).WithTenantId(tenantId).WithClientSecret(secret).Build();
				app.AddInMemoryTokenCache();

				// With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
				// application permissions need to be set statically (in the portal or by PowerShell), and then granted by
				// a tenant administrator. 
				string[] scopes = new string[] { $"{apiUrl}.default" };

				AuthenticationResult result = null;
				try
				{
					result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

					log.LogInformation("Token acquired");
				}
				catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
				{
					// Invalid scope. The scope has to be of the form "https://resourceurl/.default"
					// Mitigation: change the scope to be as expected
					log.LogError("Scope provided is not supported");
				}
				string m365Token = result.AccessToken;
				return (m365Token);
			}
			catch
			{
				log.LogError("Error recovering M365 Authentication Token");				
				throw ;
			}
		}


		public static int SaveCSV(string result, string fileName, ILogger log)
		{
			try
			{
				string filePath = "C:/home/data/";
				string fullFileName = filePath + fileName;

				using (var output = new StreamWriter(fullFileName, false, Encoding.Unicode))
				{
					output.WriteLine(result);
					output.Close();
				}
				File.WriteAllLines(fullFileName, File.ReadAllLines(fullFileName).Where(l => !string.IsNullOrWhiteSpace(l)), Encoding.Unicode);
				log.LogInformation($"generated file: {fileName}");
				return (0);
			}
			catch
			{
				log.LogError($"Error saving {fileName}");
				throw;
			}
		}

		public static string RemoveCommas(string data, ILogger log)
		{
			try
			{
				log.LogDebug("Removing unwanted commas");
				string result = "";
				string[] lines = data.Split("\r\n");
				foreach (string line in lines)
				{
					string quotes = "\"";
					if (line.Contains(quotes))
					{
						int startIndex = line.IndexOf(quotes, 0) + quotes.Length;
						int endIndex = line.IndexOf(quotes, startIndex);
						string originalSubstring = line.Substring(startIndex, endIndex - startIndex);
						string newSubstring = originalSubstring.Replace(",", "¬");
						string newline = line.Replace(originalSubstring, newSubstring);
						result = result + newline + "\r\n";
					}
					else
					{
						result = result + line + "\r\n";
					}

				}
				return (result);
			}
			catch
			{
				log.LogError("Error removing commas");
				throw;
			}
		}

		public static string AddTenantId(string data, ILogger log)
		{
			try
			{
				log.LogDebug("Adding Tenant Information");
				string tenantId = GetSecret("TenantID", log).Value;
				string result = "";
				string[] lines = data.Split("\r\n");
				int index = 0;
				foreach (string line in lines)
				{
					if (index == 0)
					{
						result = line + ",tenantId\r\n";
						index++;
					}
					else
					{

						if (line.Length != 0)
						{
							result = result + line + "," + tenantId + "\r\n";
						}

					}
				}
				return (result);
			}
			catch
			{
				log.LogError("Error adding TenantId data");
				throw;
			}
		}

		public static string AddTimestamp(string data, ILogger log)
		{
			try
			{
				log.LogDebug("Adding TimeStamp information");
				string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
				string result = "";
				string[] lines = data.Split("\r\n");
				int index = 0;
				foreach (string line in lines)
				{
					if (index == 0)
					{
						result = line + ",timeStamp\r\n";
						index++;
					}
					else
					{
						if (line.Length != 0)
						{
							result = result + line + "," + timeStamp + "\r\n";
						}
					}
				}
				return (result);
			}
			catch
			{
				log.LogError("Error adding TimeStamp data");
				throw;
			}
		}

		public static async Task<String> GetM365Data(string accessToken, string webApiUrl, ILogger log)
		{
			try
			{
				log.LogInformation("Getting data from API...");

				bool throttling = false;
				var httpClient = new HttpClient();
				HttpResponseMessage response;
				var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
				defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				do
				{
					response = await httpClient.GetAsync(webApiUrl);
					log.LogDebug($"HTTP Statuscode: { response.StatusCode}");
					switch ((int)response.StatusCode)
					{
						case 429:
							log.LogWarning("Throttling Exception. Waiting 5 seconds");
							throttling = true;
							Thread.Sleep(5000);
							break;

						default:
							throttling = false;
							break;
					}
				}
				while (throttling);
				string result = await response.Content.ReadAsStringAsync();
				return (result);
			}
			catch
			{
				log.LogError($"Error querying {webApiUrl}");				
				throw;
			}
		}

// Function GenerateSASToken not needed anymore. Remove y followin versions.
// Luise, 2022-06-01

		public static void GenerateSASToken(ILogger log)

		{
			try
			{
				log.LogDebug("Recovering parameters");
				string accountName = Environment.GetEnvironmentVariable("DataLakeName");

				string storageKey = GetSecret("DataLakeKey", log).Value.ToString();

				// Setup an AccountSASBuilder with the needed permissions and parameters				
				AccountSasBuilder sasBuilder = new AccountSasBuilder()
				{
					Services = AccountSasServices.All,
					ResourceTypes = AccountSasResourceTypes.All,
					ExpiresOn = DateTimeOffset.UtcNow.AddDays(5),
					Protocol = SasProtocol.Https
				};

				sasBuilder.SetPermissions(AccountSasPermissions.All);

				// Use the key to get the SAS token.
				Azure.Storage.StorageSharedKeyCredential storageSharedKeyCredential = new Azure.Storage.StorageSharedKeyCredential(accountName,storageKey);
				
				var sasToken = sasBuilder.ToSasQueryParameters(storageSharedKeyCredential);
				log.LogDebug("Token acquired");
				
				// Build the query toupdate SQL external resources access with the generated SAS Token
				string sqlQuery = $"exec sp_create_external_resources '{accountName}','{sasToken}'";
				RunSQLQuery(sqlQuery, log);
				log.LogDebug("Token inserted in SQL");
			}
			catch (Exception e)
			{
				log.LogError("Could not generate Token or load it into SQL");
				log.LogError(e.Message);
			}
		}

		public static int WakeUpSQLDatabase(ILogger log)
		{
			try
			{
				log.LogInformation("Waking up database if sleeping");
				string query = "select name from sys.tables";
				int alive = 0; //This means that the Database is stopped
				int watchDog = 0; //Don't want to be checking the database forever.
				do
				{
					string connectionString = Common.GetSecret("SQLconnectionString", log).Value.ToString();

					log.LogInformation($"{query}");
					SqlConnection connection = new SqlConnection(connectionString);

					SqlCommand command = new SqlCommand();
					command.Connection = connection;
					command.CommandTimeout = 0;
					command.CommandType = CommandType.Text;
					command.CommandText = query;

					try
					{
						connection.Open();
						log.LogInformation($"Executing StoredProcedure {query}");
						SqlDataReader reader = command.ExecuteReader();
						log.LogInformation($"Executed StoredProcedure {query}");
						alive = 1;
					}
					catch
					{
						alive = 0;
						Thread.Sleep(10000);
					}
					watchDog++;

				}
				while (alive == 0 && watchDog <= 20);
				if (alive == 0)
				{
					log.LogError("Database not ready");
					throw new InvalidOperationException("Database not ready");
				}
				else
				{
					log.LogInformation("Database Ready");
				}
				return (0);
			}
			catch (Exception e)
			{
				log.LogError(e.Message);
				throw;
			}
		}

		public static void RunStoredProcedure(string storedProcedure, ILogger log)
		{
			try
			{
				string connectionString = Common.GetSecret("SQLconnectionString", log).Value.ToString();

				log.LogInformation($"{storedProcedure}");
				SqlConnection connection = new SqlConnection(connectionString);

				SqlCommand command = new SqlCommand();
				command.Connection = connection;
				command.CommandTimeout = 0;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = storedProcedure;


				try
				{
					connection.Open();
					log.LogInformation($"Executing StoredProcedure {storedProcedure}");
					SqlDataReader reader = command.ExecuteReader();
					log.LogInformation($"Executed StoredProcedure {storedProcedure}");
				}
				catch (Exception e)
				{
					log.LogError($"Error executing StoredProcedure {storedProcedure}");
					log.LogWarning(e.ToString());
				}
			}
			catch (Exception e)
            {
				log.LogError (e.ToString());
            }
		}

		public static void RunSQLQuery(string query, ILogger log)
		{
			try
			{
				string connectionString = Common.GetSecret("SQLconnectionString", log).Value.ToString();

				log.LogInformation($"{query}");
				SqlConnection connection = new SqlConnection(connectionString);

				SqlCommand command = new SqlCommand();
				command.Connection = connection;
				command.CommandTimeout = 0;
				command.CommandType = CommandType.Text;
				command.CommandText = query;


				try
				{
					connection.Open();
					log.LogInformation($"Executing StoredProcedure {query}");
					SqlDataReader reader = command.ExecuteReader();
					log.LogInformation($"Executed StoredProcedure {query}");
				}
				catch (Exception e)
				{
					log.LogError($"Error executing StoredProcedure {query}");				
				}
			}
			catch (Exception e)
			{
				log.LogError(e.ToString());
			}
		}

		public static int SqlInsert(string query, ILogger log)
		{

			KeyVaultSecret connectionString = Common.GetSecret("connectionString",log);
			SqlConnection connection = new SqlConnection(connectionString.Value.ToString());
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandTimeout = 172800;
			command.CommandType = CommandType.Text;
			command.CommandText = query;
			try
			{
				connection.Open();
				if (connection.State == ConnectionState.Open)
				{
					// Execute the query for this nothing is returned  
					command.ExecuteScalar();
				}
			}
			catch (Exception e)
			{
				log.LogWarning("Error on query:");
				log.LogWarning(query);
				log.LogError(e.Message);
				throw;

			}
			finally
			{
				Console.Write("Table TEMP_GPC_UsageDetails created");
				connection.Close();
			}
			return (0);
		}

		public static int FinalClean(string container, ILogger log)
		{
			try
			{
				string storageAccount = Environment.GetEnvironmentVariable("DataLakeName");

				var uri = "https://" + storageAccount + ".blob.core.windows.net/" + container;

				var client1 = new BlobContainerClient(new Uri(uri), new DefaultAzureCredential());
				log.LogInformation("connected to DataLake.");
				foreach (Azure.Storage.Blobs.Models.BlobItem blobItem in client1.GetBlobs())
				{
					try
					{
						client1.DeleteBlobIfExists(blobItem.Name);
						log.LogInformation(blobItem.Name + " deleted");
					}
					catch
					{
						log.LogWarning("Can't delete " + blobItem.Name + " or doesn't exists on this BLOB");
					}
				}

				string query = "BEGIN TRY " +
									$"DROP TABLE [TEMP_{container}_Usagedetails] " +
								$"END TRY " +
								$"BEGIN CATCH " +
								$"END CATCH ";
				SqlInsert(query, log);
				log.LogInformation($"Temporary {container} database table cleaned");

				return (0);
			}
			catch (Exception e)
			{
				log.LogError("Error doing the final clean");
				log.LogError(e.Message);
				return (1);
			}
		}

        public static async Task<bool> FileExists(string cloud, string blobName, ILogger log)   //Check if a file exists in the container
        {
            try
            {
                log.LogInformation($"Checking if file {blobName} exists in container {cloud}");

                string storageAccount = Environment.GetEnvironmentVariable("DataLakeName");
                string container = cloud;
                var uri = "https://" + storageAccount + ".blob.core.windows.net/" + container;

                var containerClient = new BlobContainerClient(new Uri(uri), new DefaultAzureCredential());
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                bool exists = await blobClient.ExistsAsync();
				return (exists);
            }
            catch
            {
                log.LogError($"Error accessing DataLake");
                throw;
            }

        }
    }
}
