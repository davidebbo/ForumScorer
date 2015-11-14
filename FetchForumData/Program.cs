using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ForumModels;
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

            string usersFile = Path.Combine(appDataFolder, "users.json");
            string userJson = File.ReadAllText(usersFile);
            var users = JsonConvert.DeserializeObject<List<User>>(userJson);

            foreach (var user in users)
            {
                Console.WriteLine($"Processing {user.Name}");
                try
                {
                    user.CalculateScores();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }
            }

            File.WriteAllText(Path.Combine(appDataFolder, "data.json"), JsonConvert.SerializeObject(users, Formatting.Indented));
            return 0;
        }
    }
}
