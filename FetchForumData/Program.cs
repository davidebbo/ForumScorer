using System;
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
            var scores = File.ReadAllLines(usersFile).Select(u => GetUserData(u));

            File.WriteAllText(Path.Combine(appDataFolder, "data.json"), JsonConvert.SerializeObject(scores, Formatting.Indented));
            return 0;
        }

        private static UserData GetUserData(string user)
        {
            var userData = new UserData { Name = user };

            GetUserScore(userData);

            return userData;
        }

        private static void GetUserScore(UserData userData)
        {
            Console.WriteLine($"Processing user '{userData.Name}'");
            try
            {
                var root = XElement.Load($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={userData.Name}");
                GetScoreFromRss(userData, root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                userData.WeekScore = -1;
            }
        }

        private static void GetScoreFromRss(UserData userData, XElement root)
        {
            XNamespace ns = "";

            var today = DateTime.UtcNow.Date;
            int dayOfWeek = (int)today.DayOfWeek;
            dayOfWeek = (dayOfWeek + 1) % 7;    // We wrap the week Friday night (utc)
            DateTime firstDayOfThisWeek = today.AddDays(-dayOfWeek);
            DateTime firstDayOfLastWeek = firstDayOfThisWeek.AddDays(-7);

            var titles = new HashSet<string>();

            foreach (var item in root.Descendants(ns + "item"))
            {
                string pubDate = item.Descendants(ns + "pubDate").FirstOrDefault()?.Value;
                string title = item.Descendants(ns + "title").FirstOrDefault()?.Value;

                // Don't process if title is identical
                if (titles.Contains(title)) continue;

                titles.Add(title);

                var dt = DateTime.Parse(pubDate);

                int newScore = 0;

                if (title.StartsWith("Quickly answered the question"))
                {
                    newScore += 20;
                }
                else if (title.StartsWith("Answered the question"))
                {
                    newScore += 15;
                }
                else if (title.StartsWith("Contributed a helpful post"))
                {
                    newScore += 5;
                }
                else if (title.StartsWith("Replied to a forums thread"))
                {
                    newScore += 1;
                }

                if (dt > firstDayOfThisWeek)
                {
                    userData.WeekScore += newScore;
                }
                else if (dt > firstDayOfLastWeek)
                {
                    userData.PreviousWeekScore += newScore;
                }
                else
                {
                    // Older than we process
                    break;
                }
            }
        }

        class UserData
        {
            public string Name { get; set; }
            public int WeekScore { get; set; }
            public int PreviousWeekScore { get; set; }
        }
    }
}
