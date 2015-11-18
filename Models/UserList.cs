using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ForumModels
{
    public class UserList
    {
        List<User> _users;

        public DateTimeOffset FirstDayOfLastWeek { get; }
        public DateTimeOffset FirstDayOfThisWeek { get; }

        public int FirstStackOverflowPostForLastWeek { get; }
        public int FirstStackOverflowPostForThisWeek { get; }

        public UserList(string usersFile)
        {
            var today = DateTimeOffset.UtcNow.Date;
            int dayOfWeek = (int)today.DayOfWeek;
            dayOfWeek = (dayOfWeek + 1) % 7;    // We wrap the week Friday night (utc)
            FirstDayOfThisWeek = today.AddDays(-dayOfWeek);
            FirstDayOfLastWeek = FirstDayOfThisWeek.AddDays(-7);

            // We only count events attached to posts (questions or answers) that we created at most 3 days before
            // the week starts. The idea is to not keep rewarding users for old entries that are getting upticks
            FirstStackOverflowPostForThisWeek = QueryHelpers.GetIdOfFirstPostOfDay(FirstDayOfThisWeek.AddDays(-3));
            FirstStackOverflowPostForLastWeek = QueryHelpers.GetIdOfFirstPostOfDay(FirstDayOfLastWeek.AddDays(-3));

            string userJson = File.ReadAllText(usersFile);
            _users = JsonConvert.DeserializeObject<List<User>>(userJson);
        }

        public Task CalculateScores()
        {
            // Calculate the scores for all users in parallel
            return Task.WhenAll(_users.Select(user => user.CalculateScores(this)));
        }

        public void SaveDataFile(string dataFilePath)
        {
            File.WriteAllText(dataFilePath, JsonConvert.SerializeObject(_users, Formatting.Indented));
        }
    }
}
