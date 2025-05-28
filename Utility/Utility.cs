using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using ADOAnalyser.Models;


namespace ADOAnalyser
{
    public class Utility : IUtility
    {
        private static HttpClient client;

        private static string baseUrl = "https://dev.azure.com/civica-cp/";

        public Utility(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient("AzureDevOpsClient");
        }

        public async Task<string> GetDataAsync(string URL)
        {
            try
            {
                var response = await client.GetAsync(URL);

                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    return stringResponse;
                }
                else
                {
                    return @"{'Error':'Error'}";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetDataSync(string URL)
        {
            try
            {
                URL = baseUrl + URL;
                var response = client.GetAsync(URL).Result;
                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    return stringResponse;
                }
                else
                {
                    return @"{'Error':'Error'}";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> PostDataAsync(string URL, HttpContent content)
        {
            try
            {
                URL = baseUrl + URL;
                var response = await client.PostAsync(URL, content);
                if (!response.IsSuccessStatusCode)
                {
                      return @"{'Error':'Error'}";
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string PostDataSync(string URL, HttpContent content)
        {
            try
            {
                URL = baseUrl + URL;
                var response =  client.PostAsync(URL, content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return @"{'Error':'Error'}";
                }
                else
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
