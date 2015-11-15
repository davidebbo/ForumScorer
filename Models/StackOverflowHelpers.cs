using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ForumModels
{
    public static class StackOverflowHelpers
    {
        static string _stackOverflowKey;
        static HttpClient _httpClient;

        static StackOverflowHelpers()
        {
            _stackOverflowKey = ConfigurationManager.AppSettings["StackOverflowKey"];
            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
        }

        public static async Task<dynamic> Query(string path, string query)
        {
            string queryUrl = $"http://api.stackexchange.com{path}?key={_stackOverflowKey}&site=stackoverflow&{query}";
            string json = await _httpClient.GetStringAsync(queryUrl);
            return JsonConvert.DeserializeObject<dynamic>(json).items;
        }
    }
}
