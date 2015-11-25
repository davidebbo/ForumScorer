using System;
using System.IO;
using ForumModels;

namespace FetchForumData
{
    class Program
    {
        static int Main(string[] args)
        {
            // A date is optionally passed in on command line. e.g. "11/18/2015"
            DateTimeOffset startOfWeek = Helpers.GetStartOfWeek(args.Length > 0 ? args[0] : null);

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
            var userList = new UserList(usersFile, startOfWeek);
            userList.CalculateScores().Wait();
            userList.SaveDataFile(appDataFolder);

            return 0;
        }
    }
}
