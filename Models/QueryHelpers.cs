using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ForumModels
{
    public static class QueryHelpers
    {
        static string _stackOverflowKey;
        static HttpClient _httpClient;

        static QueryHelpers()
        {
            _stackOverflowKey = ConfigurationManager.AppSettings["StackOverflowKey"];
            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
        }

        public static async Task<string> GetStringAsync(string requestUri)
        {
            return await _httpClient.GetStringAsync(requestUri);
        }

        public static async Task<dynamic> Query(string path, string query)
        {
            string queryUrl = $"http://api.stackexchange.com{path}?key={_stackOverflowKey}&site=stackoverflow&{query}";
            string json = await _httpClient.GetStringAsync(queryUrl);
            return JsonConvert.DeserializeObject<dynamic>(json).items;
        }

        public static int GetIdOfFirstPostOfDay(DateTimeOffset day)
        {
            var items = Query(
                $"/2.2/posts",
                $"pagesize=1&order=asc&min={day.ToUnixTimeSeconds()}&max={day.AddDays(1).ToUnixTimeSeconds()}&sort=creation").Result;

            return items[0].post_id;
        }
    }
}
