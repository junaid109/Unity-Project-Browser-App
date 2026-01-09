using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface ILearnService
    {
        Task<IEnumerable<LearnContent>> SearchContentAsync(string query = "");
    }

    public class LearnService : ILearnService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://learn.unity.com/api/learn/search";

        public LearnService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<IEnumerable<LearnContent>> SearchContentAsync(string query = "")
        {
            try
            {
                // Construct query: ?k=["q:query","lang:en"]&pageSize=20
                var k = string.IsNullOrEmpty(query) 
                    ? "[\"lang:en\"]" 
                    : $"[\"q:{query}\",\"lang:en\"]";

                var url = $"{BaseUrl}?k={Uri.EscapeDataString(k)}&pageSize=20";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var root = JsonNode.Parse(jsonString);

                var results = new List<LearnContent>();
                var resultsArray = root?["results"]?.AsArray();

                if (resultsArray != null)
                {
                    foreach (var item in resultsArray)
                    {
                        var type = item?["type"]?.ToString();
                        if (string.IsNullOrEmpty(type)) continue;

                        var data = item?[type]; // The actual content is nested under the type name (e.g., item["tutorial"])

                        if (data != null)
                        {
                            var content = new LearnContent
                            {
                                Id = data["id"]?.ToString(),
                                Title = data["title"]?.ToString(),
                                Description = data["descPlain"]?.ToString() ?? data["description"]?.ToString(),
                                ThumbnailUrl = data["thumbnail"]?["url"]?.ToString(),
                                Url = $"https://learn.unity.com/{type}/{data["slug"]?.ToString()}",
                                Duration = data["duration"]?.GetValue<int>() ?? 0,
                                SkillLevel = data["skillLevel"]?.ToString(),
                                Type = type
                            };
                            results.Add(content);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch learn content: {ex.Message}");
                return new List<LearnContent>();
            }
        }
    }
}
