using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CloudInform
{
    internal class AzureAPI
    {
        private static string GetReservedInstancesUsageDetails(string enrollment, string token, string startDate, string endDate, ILogger log)
        {
            try
            {

                string request = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{enrollment}/providers/Microsoft.Consumption/reservationDetails?api-version=2019-10-01&%24filter=properties%2FusageDate%20ge%20{startDate}%20AND%20properties%2FusageDate%20le%20{endDate}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var req = new HttpRequestMessage(HttpMethod.Get, request);

                using var res = client.SendAsync(req).Result;
                var jsonString = res.Content.ReadAsStringAsync().Result;

                return jsonString;
            }
            catch 
            {
                throw;
            }
        }

        private static async void WriteReservedInstancesUsageDetails(string enrollment, string bearer, string startDate, string endDate, ILogger log)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");

                var jsonString = GetReservedInstancesUsageDetails(enrollment, bearer, startDate, endDate, log);

                log.LogDebug("Writting ReservedInstances Usage Details to file.");
                var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
                dynamic values = json.value;

                string fileName = $"RIUsageDetails-{enrollment}-{endDate}.csv";
                string fullFileName = $"c:/home/data/{fileName}";   // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
                var output = new StreamWriter(fullFileName, false, Encoding.Unicode);

                output.WriteLine("reservationOrderId#reservationId#usageDate#skuName#instanceId#totalReservedQuantity#reservedHours#usedHours#instanceFlexibilityGroup#instanceFlexibilityRatio");

                foreach (var value in values)
                {
                    output.WriteLine($"{value.properties.reservationOrderId}#{value.properties.reservationId}#{value.properties.usageDate}#{value.properties.skuName}#{value.properties.instanceId}#{value.properties.totalReservedQuantity}#{value.properties.reservedHours}#{value.properties.usedHours}#{value.properties.instanceFlexibilityGroup}#{value.properties.instanceFlexibilityRatio}");
                }
                output.Close();

                await Common.UploadBlob("azure", fileName, log);
                return;
            }
            catch (Exception e)
            {
                log.LogError("Line 67");
                log.LogDebug(e.Message.ToString());
                throw;
            }
        }

        private static string GetReservedInstancesUsageSummary(string enrollment, string token, string startDate, string endDate, ILogger log)
        {
            try
            {

                string request = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{enrollment}/providers/Microsoft.Consumption/reservationSummaries?grain=daily&%24filter=properties/usageDate ge {startDate} AND properties/usageDate le {endDate}&api-version=2019-10-01";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var req = new HttpRequestMessage(HttpMethod.Get, request);

                using var res = client.SendAsync(req).Result;
                var jsonString = res.Content.ReadAsStringAsync().Result;

                return jsonString;
            }
            catch 
            {
                throw;
            }
        }

        private static async void WriteReservedInstancesUsageSummary(string enrollment, string bearer, string startDate, string endDate, ILogger log)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMM-dd");

                var jsonString = GetReservedInstancesUsageSummary(enrollment, bearer, startDate, endDate, log);

                log.LogDebug("Writting ReservedInstances Usage Summary to file.");
                var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
                dynamic values = json.value;

                string fileName = $"RIUsageSummary-{enrollment}-{endDate}.csv";
                string fullFileName = $"c:/home/data/{fileName}";   // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
                var output = new StreamWriter(fullFileName, false, Encoding.Unicode);

                output.WriteLine("reservationOrderId#reservationId#usageDate#skuName#kind#reservedHours#usedHours#minUtilizationPercentage#avgUtilizationPercentage#maxUtilizationPercentage#purchasedQuantity#remainingQuantity#totalReservedQuantity#usedQuantity#utilizedPercentage");

                foreach (var value in values)
                {
                    output.WriteLine($"{value.properties.reservationOrderId}#{value.properties.reservationId}#{value.properties.usageDate}#{value.properties.skuName}#{value.properties.kind}#{value.properties.reservedHours}#{value.properties.usedHours}#{value.properties.minUtilizationPercentage}#{value.properties.avgUtilizationPercentage}#{value.properties.maxUtilizationPercentage}#{value.properties.purchasedQuantity}#{value.properties.remainingQuantity}#{value.properties.totalReservedQuantity}#{value.properties.usedQuantity}#{value.properties.utilizedPercentage}");
                }
                output.Close();

                await Common.UploadBlob("azure", fileName, log);
                return;
            }
            catch (Exception e)
            {
                log.LogError("Line 122");
                log.LogDebug(e.Message.ToString());
                throw;
            }
        }


        private static string GetReservedInstancesCostSummary(string enrollment, string token, string startDate, string endDate, ILogger log)
        {
            try
            {
                string request = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{enrollment}/providers/Microsoft.Consumption/reservationTransactions?api-version=2019-10-01&%24filter=properties%2FeventDate%20ge%20{startDate}%20and%20properties%2FeventDate%20le%20{endDate}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var req = new HttpRequestMessage(HttpMethod.Get, request);

                using var res = client.SendAsync(req).Result;
                var jsonString = res.Content.ReadAsStringAsync().Result;

                return jsonString;
            }
            catch 
            {
                throw;
            }
        }

        private static async void WriteReservedInstancesCostSummary(string enrollment, string bearer, string startDate, string endDate, ILogger log)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");

                var jsonString = GetReservedInstancesCostSummary(enrollment, bearer, startDate, endDate, log);

                log.LogDebug("Writting ReservedInstances Cost Summary to file.");
                var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
                dynamic values = json.value;

                string fileName = $"RICostSummary-{enrollment}-{endDate}.csv";
                string fullFileName = $"c:/home/data/{fileName}";   // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
                var output = new StreamWriter(fullFileName, false, Encoding.Unicode);

                output.WriteLine("billingFrequency#purchasingEnrollment#armSKUName#term#region#purchasingSubscriptionName#eventDate#reservationOrderId#description#quantity#amount#currency#reservationOrderName");

                foreach (var value in values)
                {
                    output.WriteLine($"{value.properties.billingFrequency}#{value.properties.purchasingEnrollment}#{value.properties.armSkuName}#{value.properties.term}#{value.properties.region}#{value.properties.purchasingSubscriptionName}#{value.properties.eventDate}#{value.properties.reservationOrderId}#{value.properties.description}#{value.properties.quantity}#{value.properties.amount}#{value.properties.currency}#{value.properties.reservationOrderName}");
                }
                output.Close();

                await Common.UploadBlob("azure", fileName, log);

                return;
            }
            catch (Exception e)
            {
                log.LogError("Line 180");
                log.LogDebug(e.Message.ToString());
                throw;
            }
        }

        private static string GetReservedInstancesReservationOrders(string enrollment, string token, string startDate, string endDate, ILogger log)
        {
            try
            {
                string request = $"https://management.azure.com/providers/Microsoft.Capacity/reservationOrders?api-version=2019-04-01";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var req = new HttpRequestMessage(HttpMethod.Get, request);

                using var res = client.SendAsync(req).Result;
                var jsonString = res.Content.ReadAsStringAsync().Result;

                return jsonString;
            }
            catch 
            {
                throw;
            }
        }

        private static async void WriteReservedInstancesReservationOrders(string enrollment, string bearer, string startDate, string endDate, ILogger log)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMM");

                var jsonString = GetReservedInstancesReservationOrders(enrollment, bearer, startDate, endDate, log);

                log.LogDebug("Writting Reservation Orders to file.");
                var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
                dynamic values = json.value;

                string fileName = $"RIReservationOrders-{enrollment}-{endDate}.csv";
                string fullFileName = $"c:/home/data/{fileName}";   // fullFileName is an static assignment. Is the path in Azure Functions Host where writing is allowed
                var output = new StreamWriter(fullFileName, false, Encoding.Unicode);

                output.WriteLine("displayName#createdDateTime#expiryDate#term#billingPlan");

                foreach (var value in values)
                {
                    output.WriteLine($"{value.properties.displayName}#{value.properties.createdDateTime}#{value.properties.expiry}#{value.properties.term}#{value.properties.billingPlan}");
                }
                output.Close();

                await Common.UploadBlob("azure", fileName, log);

                return;
            }
            catch (Exception e)
            {
                log.LogError("Line 180");
                log.LogDebug(e.Message.ToString());
                throw;
            }
        }


        public static void GetAzureData(DateTime startDate, DateTime endDate, ILogger log)
        {
            try 
            {
                string appId = Common.GetSecret("ApplicationID", log).Value;
                string secret = Common.GetSecret("ApplicationSecret", log).Value;
                string tenantId = Common.GetSecret("TenantID", log).Value;

                string enrollmentsList = Environment.GetEnvironmentVariable("Enrollment");

               
                log.LogInformation($"Found Enrollments: {enrollmentsList}");
                string[] enrollments = enrollmentsList.Split(',');
                foreach (string enrollment in enrollments)
                {
                    log.LogInformation($"Processing enrollment: {enrollment}");
                    string token = Common.GetBearer(tenantId, appId, secret, "https://management.azure.com/", log);

                    WriteReservedInstancesUsageDetails(enrollment, token, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), log);

                    WriteReservedInstancesUsageSummary(enrollment, token, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), log);

                    WriteReservedInstancesCostSummary(enrollment, token, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), log);

                   // WriteReservedInstancesReservationOrders(enrollment,token, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), log);

                }


            } 
            catch
            { 
            }
       }
    }
}