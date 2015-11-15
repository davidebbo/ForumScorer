﻿using System;
using System.IO;
using ForumModels;

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
            var userList = new UserList(usersFile);
            userList.CalculateScores();
            userList.SaveDataFile(Path.Combine(appDataFolder, "data.json"));

            return 0;
        }
    }
}
