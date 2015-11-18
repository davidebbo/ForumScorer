using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task CalculateScores(UserList userList)
        {
            Console.WriteLine($"Processing {Name}");

            _userList = userList;

            try {
                await Task.WhenAll(CalculateMSDNScore(), CalculateStackOverflowScores());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error for user {Name}: " + e);
            }
        }

        async Task CalculateMSDNScore()
        {
            if (String.IsNullOrWhiteSpace(MSDNName))
                return;

            try
            {
                string rssXml = await QueryHelpers.GetStringAsync($"https://social.msdn.microsoft.com/Profile/u/activities/feed?displayName={MSDNName}");
                var root = XElement.Parse(rssXml);
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
                    newScore += 20;
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

        async Task CalculateStackOverflowScores()
        {
            if (String.IsNullOrWhiteSpace(StackOverflowID))
                return;

            // Calculate the previous and current week's scores in parallel
            var stackOverflowScores = await Task.WhenAll(
                GetStackOverflowReputation(_userList.FirstDayOfLastWeek, _userList.FirstDayOfThisWeek, _userList.FirstStackOverflowPostForLastWeek),
                GetStackOverflowReputation(_userList.FirstDayOfThisWeek, DateTimeOffset.UtcNow, _userList.FirstStackOverflowPostForThisWeek));

            PreviousWeekScores.StackOverflow += stackOverflowScores[0];
            WeekScores.StackOverflow += stackOverflowScores[1];
        }

        async Task<int> GetStackOverflowReputation(DateTimeOffset start, DateTimeOffset end, int firstPostIdToConsider)
        {
            dynamic items = await QueryHelpers.Query(
                $"/2.2/users/{StackOverflowID}/reputation",
                $"fromdate={start.ToUnixTimeSeconds()}&todate={end.ToUnixTimeSeconds()}");

            int reputation = 0;
            foreach (var item in items)
            {
                // Ignore if the post that the event is attached to is too old
                if (item.post_id < firstPostIdToConsider)
                    continue;

                if (item.vote_type == "up_votes")
                {
                    if (item.post_type == "answer")
                    {
                        // Cap it at 20 (i.e. two upvotes)
                        reputation += Math.Min((int)item.reputation_change, 20);
                    }
                    else
                    {
                        // Cap it at 10 (i.e. two upvotes)
                        reputation += Math.Min((int)item.reputation_change, 10);
                    }
                }
                else if (item.vote_type == "accepts")
                {
                    reputation += 15;
                }
                else
                {
                    // We ignore bounties
                }
            }

            return reputation;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
