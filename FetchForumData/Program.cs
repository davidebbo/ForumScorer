using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FetchForumData
{
    class Program
    {
        static int Main(string[] args)
        {
            // App_Data folder in Azure
            string appDataFolder = @"D:\home\site\wwwroot\app_data";
            if (!Directory.Exists(appDataFolder))
            {
                // App_Data folder in local development
                appDataFolder = Path.GetFullPath(@"..\..\..\ForumScorer\App_Data");
                if (!Directory.Exists(appDataFolder))
                {
                    Console.WriteLine($"Can't find App_Data folder '{appDataFolder}'.");
                    return 1;
                }
            }

            string usersFile = Path.Combine(appDataFolder, "users.txt");
            var scores = File.ReadAllLines(usersFile).Select(u => GetUserData(u)).OrderByDescending(d => d.Score);

            Console.WriteLine(JsonConvert.SerializeObject(scores, Formatting.Indented));
            return 0;
        }

        private static UserData GetUserData(string user)
        {
            return new UserData
            {
                Name = user,
                Score = 0
            };
        }

        class UserData
        {
            public string Name { get; set; }
            public int Score { get; set; }
        }
    }
}
