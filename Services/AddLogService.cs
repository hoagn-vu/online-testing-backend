#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class AddLogService
    {
        private readonly IMongoCollection<UsersModel> _users;

        public AddLogService() { }

        public AddLogService(IMongoDatabase database)
        {
            this._users = database.GetCollection<UsersModel>("users");
        }

        public async Task AddActionLog(string userId, UserLogsModel logData)
        {
            var user = await this._users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return;
            }

            logData.LogId = ObjectId.GenerateNewId().ToString();

            if (user.UserLog == null)
            {
                user.UserLog = new List<UserLogsModel>();
            }

            user.UserLog.Add(logData);

            var update = Builders<UsersModel>.Update.Push(u => u.UserLog, logData);
            await this._users.UpdateOneAsync(u => u.Id == userId, update);
        }
    }
}
