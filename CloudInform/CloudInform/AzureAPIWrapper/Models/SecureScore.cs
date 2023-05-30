using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CloudInform.AzureAPIWrapper.Models
{
    public class SecureScore
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonProperty("value")]
        public List<Value> Values { get; set; }
        
        public class ControlScore
        {
            [JsonProperty("controlCategory")]
            public string ControlCategory { get; set; }

            [JsonProperty("controlName")]
            public string ControlName { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("score")]
            public double Score { get; set; }

            [JsonProperty("on")]
            public string On { get; set; }

            [JsonProperty("IsApplicable")]
            public string IsApplicable { get; set; }

            [JsonProperty("scoreInPercentage")]
            public double ScoreInPercentage { get; set; }

            [JsonProperty("implementationStatus")]
            public string ImplementationStatus { get; set; }

            [JsonProperty("lastSynced")]
            public DateTime LastSynced { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }

            [JsonProperty("count")]
            public string Count { get; set; }

            [JsonProperty("controlState")]
            public string ControlState { get; set; }

            [JsonProperty("total")]
            public string Total { get; set; }

            [JsonProperty("State")]
            public string State { get; set; }

            [JsonProperty("expiry")]
            public string Expiry { get; set; }

            [JsonProperty("noPolicies")]
            public string NoPolicies { get; set; }

            [JsonProperty("mdoImplementationStatus")]
            public string MdoImplementationStatus { get; set; }
        }

        public class Value
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("azureTenantId")]
            public string AzureTenantId { get; set; }

            [JsonProperty("activeUserCount")]
            public int ActiveUserCount { get; set; }

            [JsonProperty("createdDateTime")]
            public DateTime CreatedDateTime { get; set; }

            [JsonProperty("currentScore")]
            public double CurrentScore { get; set; }

            [JsonProperty("enabledServices")]
            public List<string> EnabledServices { get; set; }

            [JsonProperty("licensedUserCount")]
            public int LicensedUserCount { get; set; }

            [JsonProperty("maxScore")]
            public double MaxScore { get; set; }

            [JsonProperty("vendorInformation")]
            public VendorInformation VendorInformation { get; set; }

            [JsonProperty("averageComparativeScores")]
            public List<AverageComparativeScores> AverageComparativeScores { get; set; }

            [JsonProperty("controlScores")]
            public List<ControlScore> ControlScores { get; set; }
        }

        public class VendorInformation
        {
            [JsonProperty("provider")]
            public string Provider { get; set; }

            [JsonProperty("providerVersion")]
            public object ProviderVersion { get; set; }

            [JsonProperty("subProvider")]
            public object SubProvider { get; set; }

            [JsonProperty("vendor")]
            public string Vendor { get; set; }
        }


        public class AverageComparativeScores
        {
            [JsonProperty("averageScore")]
            public double AverageScore { get; set; }

            [JsonProperty("seatSizeRangeLowerValue")]
            public double SeatSizeRangeLowerValue { get; set; }

            [JsonProperty("seatSizeRangeUpperValue")]
            public double SeatSizeRangeUpperValue { get; set; }

        }
    }
}
