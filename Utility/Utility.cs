using ADOAnalyser.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static Microsoft.CodeAnalysis.IOperation;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ADOAnalyser
{
    public class Utility : IUtility
    {
        private static HttpClient client;

        private static string baseUrl = "https://dev.azure.com/civica-cp/";

        public string NormalizePath(string path) => path.Trim().Replace("/", "\\").ToLowerInvariant();

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
                var response = client.PostAsync(URL, content).Result;
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


        public IterationResult GetCurrentIterationAsync(string projectName)
        {
            var result = new IterationResult();

            var expectedPaths = new List<string>
                                {
                                    "CE\\The Mastermind",
                                    "CE\\View Warriors"
                                };

            var uri = baseUrl + string.Format("{0}/_apis/wit/classificationnodes/iterations?$depth=3&api-version=7.1-preview.2", projectName.ToUpper());

            var response = client.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
            {
                var stringResponse = response.Content.ReadAsStringAsync().Result;
                var root = JsonConvert.DeserializeObject<IterationRoot>(stringResponse);

                var normalizedPaths = expectedPaths
                                       .Select(NormalizePath)
                                       .ToList();

                var allIterations = FlattenIterationsWithPath(root.children, projectName.ToUpper());
                var today = DateTime.UtcNow.Date;

                result.AllSprints = allIterations
                                      .Where(i => normalizedPaths.Any(p => NormalizePath(i.FullPath).StartsWith(p)))
                                      .OrderByDescending(i => i.Attributes.StartDate)
                                      .ToList();

                result.CurrentSprints = result.AllSprints
                                       .Where(i =>
                                           i.Attributes?.StartDate <= today &&
                                           i.Attributes?.FinishDate >= today)
                                       .OrderByDescending(i => i.Attributes.StartDate)
                                       .ToList();

                return result;
            }
            else
            {
                return result;
            }
        }

        public List<IterationNode> FlattenIterations(List<IterationNode> nodes)
        {
            var result = new List<IterationNode>();

            foreach (var node in nodes)
            {
                if (node.Attributes?.StartDate != null && node.Attributes?.FinishDate != null)
                    result.Add(node);

                if (node.children != null && node.children.Count > 0)
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
                        Name = node.name,
                        FullPath = currentPath,
                        Attributes = node.Attributes
                    });
                }

                if (node.children != null && node.children.Any())
                {
                    var childFlattened = FlattenIterationsWithPath(node.children, currentPath);
                    list.AddRange(childFlattened);
                }
            }

            return list;
        }

        public class IterationRoot
        {
            public string name { get; set; }
            public List<IterationNode> children { get; set; }
        }

        public class IterationNode
        {
            public string name { get; set; }
            public IterationAttributes Attributes { get; set; }
            public List<IterationNode> children { get; set; }
        }

        public class IterationAttributes
        {
            public DateTime? StartDate { get; set; }
            public DateTime? FinishDate { get; set; }
        }

        public class IterationNodeWithPath
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public IterationAttributes Attributes { get; set; }
        }
    }
}
