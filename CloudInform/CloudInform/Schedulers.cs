using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CloudInform
{
    public class M365DataDownloadScheduler
    {
        [FunctionName("Scheduler-M365DataDownload")]   //Starting data recovery at 19:00:00 UTC Time
        public async Task RunAsync([TimerTrigger("0 0 19 * * *")] TimerInfo myTimer, ILogger log)
        {
            string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            string URL = $"HTTPS://{hostName}/api/M365DataDownload";
            log.LogInformation($"Launching function {URL}");
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(URL);
            log.LogInformation(response.StatusCode.ToString());
        }
    }
    public class M365DataImportScheduler
    { 
        [FunctionName("Scheduler-M365DataImport")]
        public async Task RunAsync([TimerTrigger("0 0 21 * * *")] TimerInfo myTimer, ILogger log)   //Starting data recovery at 21:00:00 UTC Time
        {
            string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            string URL = $"HTTPS://{hostName}/api/M365DataImport";
            log.LogInformation($"Launching function {URL}");
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(URL);
            log.LogInformation(response.StatusCode.ToString());
        }
    }
    public class CloudDataDownloadScheduler
    {
        [FunctionName("Scheduler-CloudDataDownload")]  
        public async Task RunAsync([TimerTrigger("0 5 0 * * *")] TimerInfo myTimer, ILogger log)  //Starting data recovery at 00:05:00 UTC Time
        {
            string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            string URL = $"HTTPS://{hostName}/api/CloudDataDownload";
            log.LogInformation($"Launching function {URL}");
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(URL);
            log.LogInformation(response.StatusCode.ToString());
        }
    }
    public class CloudDataImportScheduler
    {
        [FunctionName("Scheduler-CloudDataImport")]
        public async Task RunAsync([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer, ILogger log) //Starting data Import at 01:00:00 UTC Time
        {
            string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            string URL = $"HTTPS://{hostName}/api/CloudDataImport";
            log.LogInformation($"Launching function {URL}");
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(URL);
            log.LogInformation(response.StatusCode.ToString());
        }
    }

    public class WakeUpDatabase
    {
        [FunctionName("WakeUpDatabase")]
        public void Run([TimerTrigger("0 0 6 * * *")] TimerInfo myTimer, ILogger log) //Starting Paused Database at 06:00:00 UTC Time
        {
            log.LogInformation("Checking for online Database");
            Common.WakeUpSQLDatabase(log);
        }
    }
}


