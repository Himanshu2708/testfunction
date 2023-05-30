using CloudInform.CSPAPIWrapper.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CloudInform.CSPAPIWrapper
{
    public class CSP
    {
        public async static Task<string> GetAccessToken(string tenantId, string appId, string secret, string resourceUri)
        {
            using HttpClient client = new();
            using HttpContent requestBody = new StringContent($"resource={resourceUri}&client_id={appId}&client_secret={secret}&grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            using HttpRequestMessage request = new(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/token")
            {
                Content = requestBody
            };
            using HttpResponseMessage response = await client.SendAsync(request);
            using HttpContent content = response.Content;
            string responseBody = await content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get access token. Code: {(int)response.StatusCode} - {response.StatusCode}. Body:\r\n{responseBody}");

            TokenResponse accessTokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
            if (accessTokenResponse == null || string.IsNullOrWhiteSpace(accessTokenResponse.AccessToken))
                throw new Exception($"Failed to get access token. Code: {(int)response.StatusCode} - {response.StatusCode}. Body:\r\n{responseBody}");

            return accessTokenResponse.AccessToken;
        }

        private HttpClient Client { get; }

        public CSP(string APIBase, string Token)
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                throw new ArgumentException($"'{nameof(Token)}' cannot be null or whitespace.", nameof(Token));
            }

            Client = new HttpClient()
            {
                BaseAddress = new Uri(APIBase)
            };

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        public async Task<bool> DownloadCSPData(DateTime date, string filePath)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, $"CSPData?date={date:yyyy-MM-dd}");
            using HttpResponseMessage response = await Client.SendAsync(request);
            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    // Success no data
                    return false;
                case HttpStatusCode.OK:
                    using (HttpContent content = response.Content)
                    using (Stream stream = await content.ReadAsStreamAsync())
                    using (FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                        await stream.CopyToAsync(fileStream);
                    return true;
                default:
                    using (HttpContent content = response.Content)
                    {
                        string responseBody = await content.ReadAsStringAsync();
                        throw new Exception($"Failed to get CSP Data. Code: {(int)response.StatusCode} - {response.StatusCode}. Body:\r\n{responseBody}");
                    }
            }
        }
    }
}
