using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CloudInform
{
    internal class RBAC
    {
		private static string GetRoleAssignments(string subscriptionId, string bearer, ILogger log)
		{
			try
			{
				log.LogDebug($"Retrieving Role Assignments from subscription {subscriptionId}");
				var url = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleAssignments?api-version=2015-07-01";

				using var client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
				var req = new HttpRequestMessage(HttpMethod.Get, url);

				using var res = client.SendAsync(req).Result;
				var jsonString = res.Content.ReadAsStringAsync().Result;
				log.LogDebug($"Role Assignments from subscription {subscriptionId} retrieved");

				return jsonString;
			}
			catch (Exception e)
			{
				log.LogError("Line 94");
				log.LogError(e.Message.ToString());
				throw;
			}
		}

		private static void WriteRoleAssignements(string subscriptionId, string bearer, ILogger log)
		{
			try
			{
				string azureADName = Environment.GetEnvironmentVariable("AzureADName");
				string date = DateTime.Now.ToString("yyyyMM");

				var jsonString = GetRoleAssignments(subscriptionId, bearer, log);

				log.LogDebug("Writting Role Assignments to file.");
				var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
				dynamic roles = json.value;

				string fullFileName = $"c:/home/data/RBACpermissions-{azureADName}-{date}.csv";   // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
				var output = new StreamWriter(fullFileName, true, Encoding.Unicode);

				foreach (var role in roles)
				{
					output.WriteLine($"{role.properties.roleDefinitionId}#{role.properties.principalId}#{role.properties.scope}#{role.properties.createdOn}#{role.properties.updatedOn}#{role.properties.createdBy}#{role.properties.updatedBy}");
				}
				output.Close();
				return;
			}
			catch (Exception e)
			{
				log.LogError("Line 122");
				log.LogDebug(e.Message.ToString());
				throw;
			}
		}

		private static string GetRoleDefinitions(string subscriptionId, string bearer, ILogger log)
		{
			try
			{
				log.LogDebug($"Retrieving Roles definitions from subscription {subscriptionId}");
				var url = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions?api-version=2015-07-01";

				using var client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
				var req = new HttpRequestMessage(HttpMethod.Get, url);

				using var res = client.SendAsync(req).Result;
				var jsonString = res.Content.ReadAsStringAsync().Result;

				log.LogDebug($"Role definitions from subscription {subscriptionId} retrieved");

				return jsonString;
			}
			catch (Exception e)
			{
				log.LogError("Line 148");
				log.LogError(e.Message.ToString());
				throw;
			}
		}

		private static void WriteRolesDefinition(string subscriptionId, string bearer, ILogger log)
		{
			try
			{
				string azureADName = Environment.GetEnvironmentVariable("AzureADName");
				string date = DateTime.Now.ToString("yyyyMMdd");

				var jsonString = GetRoleDefinitions(subscriptionId, bearer, log);

				log.LogDebug("Writting Role Definitions to file.");

				var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
				dynamic roles = json.value;
				string fullFileName = $"c:/home/data/RolesDefinition-{azureADName}-{date}.csv"; // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
				var output = new StreamWriter(fullFileName, true, Encoding.Unicode);

				foreach (var role in roles)
				{
					output.WriteLine($"{role.properties.roleName}#{role.properties.type}#{role.properties.description}#{role.properties.createdOn}#{role.properties.updatedOn}#{role.properties.createdBy}#{role.properties.updatedBy}#{role.id}#{role.name}");
				}
				output.Close();
				return;
			}
			catch (Exception e)
			{
				log.LogError("Line 176");
				log.LogError(e.Message.ToString());
				throw;
			}
		}

		private static void GetSubscriptions(string bearer, ILogger log)
		{
			try
			{
				string azureADName = Environment.GetEnvironmentVariable("AzureADName");
				string date = DateTime.Now.ToString("yyyyMM");

				var url = $"https://management.azure.com/subscriptions?api-version=2020-01-01";  //To convert in a Constant
				log.LogDebug("Recovering subscriptions in Tenant");

				string fullFileName = $"c:/home/data/Subscriptions-{azureADName}-{date}.csv"; // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
				var output = new StreamWriter(fullFileName, true, Encoding.Unicode);

				using var client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
				var req = new HttpRequestMessage(HttpMethod.Get, url);

				using var res = client.SendAsync(req).Result;
				var jsonString = res.Content.ReadAsStringAsync().Result;

				var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
				dynamic subscriptions = json.value;
				foreach (dynamic subscription in subscriptions)
				{
					string subscriptionId = subscription.subscriptionId;
					output.WriteLine($"{subscription.subscriptionId}#{subscription.displayName}");

					// Role Assignments
					WriteRoleAssignements(subscriptionId, bearer, log);

					//Role Definitions
					WriteRolesDefinition(subscriptionId, bearer, log);

				}
				output.Close();
				return;
			}
			catch (Exception e)
			{
				log.LogError("Line 215");
				log.LogError(e.ToString());
				throw;
			}
		}

		private static void InitializeFiles(ILogger log)
		{
			try
			{
				string azureADName = Environment.GetEnvironmentVariable("AzureADName");
				string date = DateTime.Now.ToString("yyyyMMdd");


				string fullFileName = $"c:/home/data/RBACpermissions-{azureADName}-{date}.csv";
				var output = new StreamWriter(fullFileName, false, Encoding.Unicode);
				output.WriteLine("roleDefinitionId#principalId#scope#createdOn#updatedOn#createdBy#updatedBy");
				output.Close();

				fullFileName = $"c:/home/data/RBACRolesDefinition-{azureADName}-{date}.csv";
				output = new StreamWriter(fullFileName, false, Encoding.Unicode);
				output.WriteLine("rolename#type#description#createdOn#updatedOn#createdBy#updatedBy#id#name");
				output.Close();


				fullFileName = $"c:/home/data/Subscriptions-{azureADName}-{date}.csv";
				output = new StreamWriter(fullFileName, false, Encoding.Unicode);
				output.WriteLine("id#name");
				output.Close();
			}
			catch (Exception e)
			{
				log.LogError("line 33");
				log.LogError(e.ToString());
				throw;
			}
		}

		public static async Task GetRBACPermissions(ILogger log)
		{
			try
			{
				string azureADName = Environment.GetEnvironmentVariable("AzureADName");
				string billingMonthID = DateTime.Now.ToString("yyyyMM");

				string appId = Common.GetSecret("ApplicationID", log).Value;
				string secret = Common.GetSecret("ApplicationSecret", log).Value;
				string tenantId = Common.GetSecret("TenantID", log).Value;
				log.LogDebug("appId");

				var bearer = Common.GetBearer(tenantId, appId, secret, "https://management.azure.com/", log);
				log.LogDebug($"bearer: {bearer}");
                InitializeFiles(log);

				GetSubscriptions(bearer, log);

				await Common.UploadBlob("azure", $"Subscriptions-{azureADName}-{billingMonthID}.csv", log);

				await Common.UploadBlob("azure", $"RBACpermissions-{azureADName}-{billingMonthID}.csv", log);

				await Common.UploadBlob("azure", $"RolesDefinition-{azureADName}-{billingMonthID}.csv", log);
			}
			catch (Exception e)
			{
				log.LogDebug(e.Message);
				throw;
			}
		}
	}
}
