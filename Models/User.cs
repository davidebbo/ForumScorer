using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ForumModels
{
    public class User
    {
        UserList _userList;

        public string Name { get; set; }

        public string MSDNName { get; set; }
        public string StackOverflowID { get; set; }

        public Scores WeekScores { get; } = new Scores();
        public Scores PreviousWeekScores { get; } = new Scores();

        public void CalculateScores(UserList userList)
        {
            _userList = userList;

            CalculateMSDNScore();
            CalculateStackOverflowScores();
        }

        void CalculateMSDNScore()
        {
            if (String.IsNullOrWhiteSpace(MSDNName))
                return;

            try
            {
                var root = XElement.Load($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={MSDNName}");
                CalculateMSDNScoresFromRss(root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                WeekScores.MSDN = -1;
            }
        }

        void CalculateMSDNScoresFromRss(XElement root)
        {
            XNamespace ns = "";

            var titles = new HashSet<string>();

            foreach (var item in root.Descendants(ns + "item"))
            {
                string pubDate = item.Descendants(ns + "pubDate").FirstOrDefault()?.Value;
                string title = item.Descendants(ns + "title").FirstOrDefault()?.Value;

                // Don't process if title is identical
                if (titles.Contains(title)) continue;

                titles.Add(title);

                var dt = DateTimeOffset.Parse(pubDate);

                int newScore = 0;

                if (title.StartsWith("Quickly answered the question"))
                {
                    newScore += 25;
                }
                else if (title.StartsWith("Answered the question"))
                {
                    newScore += 20;
                }
                else if (title.StartsWith("Contributed a helpful post"))
                {
                    newScore += 10;
                }
                else if (title.StartsWith("Replied to a forums thread"))
                {
                    newScore += 1;
                }

                if (dt > _userList.FirstDayOfThisWeek)
                {
                    WeekScores.MSDN += newScore;
                }
                else if (dt > _userList.FirstDayOfLastWeek)
                {
                    PreviousWeekScores.MSDN += newScore;
                }
                else
                {
                    // Older than we process
                    break;
                }
            }
        }

        void CalculateStackOverflowScores()
        {
            if (String.IsNullOrWhiteSpace(StackOverflowID))
                return;

            PreviousWeekScores.StackOverflow += GetStackOverflowReputation(_userList.FirstDayOfLastWeek, _userList.FirstDayOfThisWeek, _userList.FirstStackOverflowPostForLastWeek);
            WeekScores.StackOverflow += GetStackOverflowReputation(_userList.FirstDayOfThisWeek, DateTimeOffset.UtcNow, _userList.FirstStackOverflowPostForThisWeek);
        }

        int GetStackOverflowReputation(DateTimeOffset start, DateTimeOffset end, int firstPostIdToConsider)
        {
            var items = StackOverflowHelpers.Query(
                $"/2.2/users/{StackOverflowID}/reputation",
                $"fromdate={start.ToUnixTimeSeconds()}&todate={end.ToUnixTimeSeconds()}").Result;

            int reputation = 0;
            foreach (var item in items)
            {
                // Ignore if the post that the event is attached to is too old
                if (item.post_id < firstPostIdToConsider)
                    continue;

                reputation += (int)item.reputation_change;
            }

            return reputation;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
