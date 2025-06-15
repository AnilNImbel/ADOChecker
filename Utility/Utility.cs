
using ADOAnalyser.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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

        public string NormalizePath(string path) => path.Trim().Replace("//", "\\").ToLowerInvariant();

        public async Task<string> GetDataAsync(string URL)
        {
            try
            {
                string apiURL = $"{baseUrl}/{URL}";
                var response = await client.GetAsync(apiURL);
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : @"{'Error':'Error'}";
            }
            catch
            {
                throw;
            }
        }

        public string GetDataSync(string URL)
        {
            try
            {
                string apiURL = $"{baseUrl}/{URL}";
                var response = client.GetAsync(apiURL).Result;
                return response.IsSuccessStatusCode
                    ? response.Content.ReadAsStringAsync().Result
                    : @"{'Error':'Error'}";
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> PostDataAsync(string URL, HttpContent content)
        {
            try
            {
                string apiURL = $"{baseUrl}/{URL}";
                var response = await client.PostAsync(apiURL, content);
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : @"{'Error':'Error'}";
            }
            catch
            {
                throw;
            }
        }

        public string PostDataSync(string URL, HttpContent content)
        {
            try
            {
                string apiURL = $"{baseUrl}/{URL}";
                var response = client.PostAsync(apiURL, content).Result;
                return response.IsSuccessStatusCode
                    ? response.Content.ReadAsStringAsync().Result
                    : @"{'Error':'Error'}";
            }
            catch
            {
                throw;
            }
        }

        public IterationResult GetCurrentIterationAsync(string projectName)
        {
            var result = new IterationResult();
            var expectedPaths = new List<string> { "CE\\The Mastermind", "CE\\View Warriors" };
            var uri = $"{baseUrl}{projectName.ToUpper()}/_apis/wit/classificationnodes/iterations?$depth=3&api-version=7.1-preview.2";

            var response = client.GetAsync(uri).Result;
            if (!response.IsSuccessStatusCode) return result;

            var stringResponse = response.Content.ReadAsStringAsync().Result;
            var root = JsonConvert.DeserializeObject<IterationRoot>(stringResponse);
            var normalizedPaths = expectedPaths.Select(NormalizePath).ToList();
            var allIterations = FlattenIterationsWithPath(root.children, projectName.ToUpper());
            var today = DateTime.UtcNow.Date;

            result.AllSprints = allIterations
                .Where(i => normalizedPaths.Any(p => NormalizePath(i.FullPath).StartsWith(p)))
                .OrderByDescending(i => i.Attributes.StartDate)
                .ToList();

            result.CurrentSprints = result.AllSprints
                .Where(i => i.Attributes?.StartDate <= today && i.Attributes?.FinishDate >= today)
                .OrderByDescending(i => i.Attributes.StartDate)
                .ToList();

            return result;
        }

        public List<IterationNode> FlattenIterations(List<IterationNode> nodes)
        {
            var result = new List<IterationNode>();
            foreach (var node in nodes)
            {
                if (node.Attributes?.StartDate != null && node.Attributes?.FinishDate != null)
                    result.Add(node);

                if (node.children?.Count > 0)
                    result.AddRange(FlattenIterations(node.children));
            }
            return result;
        }

        public List<IterationNodeWithPath> FlattenIterationsWithPath(List<IterationNode> nodes, string parentPath)
        {
            var list = new List<IterationNodeWithPath>();
            foreach (var node in nodes)
            {
                string currentPath = string.IsNullOrEmpty(parentPath) ? node.name : $"{parentPath}\\{node.name}";

                if (node.Attributes?.StartDate != null && node.Attributes?.FinishDate != null)
                {
                    list.Add(new IterationNodeWithPath
                    {
                        id = node.id,
                        Name = node.name,
                        FullPath = currentPath,
                        Attributes = node.Attributes
                    });
                }

                if (node.children?.Any() == true)
                    list.AddRange(FlattenIterationsWithPath(node.children, currentPath));
            }
            return list;
        }
    }
}
