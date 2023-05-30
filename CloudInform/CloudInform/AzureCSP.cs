using CloudInform.CSPAPIWrapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudInform
{
    internal class AzureCSP
    {
        public static async Task GetAzureCSPDataAsync(DateTime startDate, DateTime endDate, ILogger log)
        {
            try
            {
                string appId = Common.GetSecret($"CSP-ApplicationID", log).Value;
                string secret = Common.GetSecret($"CSP-ApplicationSecret", log).Value;
                string tenantId = Common.GetSecret($"CSP-TenantID", log).Value;
                string resourceUri = Common.GetSecret($"CSP-resourceUri", log).Value;
                string apiBase = Common.GetSecret($"CSP-APIBaseURL", log).Value;

                log.LogInformation($"Processing CSP^Data");
                string token;
                try
                {
                    token = await CSP.GetAccessToken(tenantId, appId, secret, resourceUri); 
                }
                catch (Exception ex)
                {    
                    log.LogError($"Error getting token: {ex.Message}");
                    return;
                }

                CSP csp = new(apiBase, token);

                for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                {
                    try
                    {
                        string path= $"C:/home/data/";
                        string fileName = $"TEMP_AZ-CSP-Usagedetails-{currentDate:yyyyMMdd}.csv";
                        string fullFileName=path+fileName ;
                        Boolean statusCode=await csp.DownloadCSPData(currentDate, fullFileName) ;
                        if (statusCode == true) //Upload to storage
                        {
                            fileName=ConvertAzureUsageDetailsFile(fileName, log);
                            //Upload to storage
                            await Common.UploadBlob("azure", fileName, log);
                        }
                        else
                        {
                            log.LogWarning($"No CSP data for day {currentDate:yyyy-MM-dd}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning($"Error getting usage details for {currentDate:yyyy-MM-dd}.");
                        log.LogError(ex.ToString());
                    }
                }
                

            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        private static string ConvertAzureUsageDetailsFile(string fileName, ILogger log)
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
 
                    int tid = 0;
                    string line;

                    while (null != (line = input.ReadLine()))
                    {

                        if (tid == 0) { line = "tid#" + line; }
                        else { line = tid + "#" + line; }  // adding tagid file
                        tid++;                       

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
    }
}


