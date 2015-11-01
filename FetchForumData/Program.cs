﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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

            File.WriteAllText(Path.Combine(appDataFolder, "data.json"), JsonConvert.SerializeObject(scores, Formatting.Indented));
            return 0;
        }

        private static UserData GetUserData(string user)
        {
            return new UserData
            {
                Name = user,
                Score = GetUserScore(user)
            };
        }

        private static int GetUserScore(string user)
        {
            Console.WriteLine($"Processing user '{user}'");
            try
            {
                var root = XElement.Load($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={user}");
                return GetScoreFromRss(root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        private static int GetScoreFromRss(XElement root)
        {
            XNamespace ns = "";
            DateTime cutoff = DateTime.Now.AddDays(-7); // Only look at past 7 days
            int score = 0;

            var titles = new HashSet<string>();

            foreach (var item in root.Descendants(ns + "item"))
            {
                string pubDate = item.Descendants(ns + "pubDate").FirstOrDefault()?.Value;
                string title = item.Descendants(ns + "title").FirstOrDefault()?.Value;

                // Don't process if title isidentical
                if (titles.Contains(title)) continue;

                titles.Add(title);

                var dt = DateTime.Parse(pubDate);
                if (dt < cutoff) break;

                if (title.StartsWith("Quickly answered the question"))
                {
                    score += 20;
                }
                else if (title.StartsWith("Answered the question"))
                {
                    score += 15;
                }
                else if (title.StartsWith("Contributed a helpful post"))
                {
                    score += 5;
                }
                else if (title.StartsWith("Replied to a forums thread"))
                {
                    score += 1;
                }
            }

            return score;
        }

        class UserData
        {
            public string Name { get; set; }
            public int Score { get; set; }
        }
    }
}