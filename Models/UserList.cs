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

        public DateTimeOffset StartOfPeriod { get; }
        public DateTimeOffset EndOfPeriod { get; }

        public int FirstStackOverflowPost { get; }

        public UserList(string usersFile, DateTimeOffset startOfWeek)
        {
            StartOfPeriod = startOfWeek.ToUniversalTime();
            EndOfPeriod = StartOfPeriod.AddDays(7);

            // We only count events attached to posts (questions or answers) that we created at most 3 days before
            // the week starts. The idea is to not keep rewarding users for old entries that are getting upticks
            FirstStackOverflowPost = QueryHelpers.GetIdOfFirstPostOfDay(StartOfPeriod.AddDays(-3));

            string userJson = File.ReadAllText(usersFile);
            _users = JsonConvert.DeserializeObject<List<User>>(userJson);
        }

        public Task CalculateScores()
        {
            // Calculate the scores for all users in parallel
            return Task.WhenAll(_users.Select(user => user.CalculateScores(this)));
        }

        public void SaveDataFile(string dataFileFolder)
        {
            string filePath = Path.Combine(dataFileFolder, Helpers.GetDataFileNameFromDate(StartOfPeriod));
            File.WriteAllText(filePath, JsonConvert.SerializeObject(_users, Formatting.Indented));
        }
    }
}
