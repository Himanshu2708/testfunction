using Microsoft.Extensions.Logging;
using System;


namespace CloudInform
{
    internal class DateFunctions
    {
        public static string LastDayOfLastMonth(ILogger log)
        {
            try
            {
                DateTime date = DateTime.Now;
                date = date.AddMonths(-1);
                string month = date.ToString("MM");
                string year = date.ToString("yyyy");
                int lastDay = DateTime.DaysInMonth(date.Year, date.Month);

                string lastDayOfLastMonth = $"{year}-{month}-{lastDay}";

                return lastDayOfLastMonth;
            }
            catch (Exception ex)
            {
                log.LogError("LastDayOfLastMonth error");
                log.LogError(ex.Message);
                throw;
            }
        }
        public static string FirstDayOfLastMonth(ILogger log)
        {
            try
            {
                DateTime date = DateTime.Now;
                date = date.AddMonths(-1);
                string month = date.ToString("MM");
                string year = date.ToString("yyyy");
                string FirstDayOfLastMonth = $"{year}-{month}-01";

                return FirstDayOfLastMonth;
            }
            catch (Exception ex)
            {
                log.LogError("FirstDayOfLastMonth error");
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}