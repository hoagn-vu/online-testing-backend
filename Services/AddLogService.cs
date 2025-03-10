using backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend_online_testing.Services
{
    public class AddLogService
    {
        private readonly IMongoCollection<UsersModel> _users;

        public AddLogService(IMongoDatabase database)
        {
            _users = database.GetCollection<UsersModel>("Users");
        }

        public async Task AddActionLog(string userId, UserLogsModel logData)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            logData.LogId = ObjectId.GenerateNewId().ToString();

            if (user.UserLog == null)
            {
                user.UserLog = new List<UserLogsModel>();
            }

            user.UserLog.Add(logData);

            var update = Builders<UsersModel>.Update.Push(u => u.UserLog, logData);
            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }
    }
}
