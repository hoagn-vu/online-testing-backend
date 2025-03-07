using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services
{
    public class UserService
    {
        private readonly IMongoCollection<UserModel> _userCollection;

        public UserService(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<UserModel>("users");
        }

        public async Task<List<UserModel>> GetUsersAsync() => await _userCollection.Find(p => !p.Status.Equals("deleted")).ToListAsync();

        public async Task CreateAsync(UserModel user, ObjectId logUserId)
        {
            user.Logs ??= [];
            user.Logs.Add(new UserLogModel { UserId = logUserId, Type = "created" });
            await _userCollection.InsertOneAsync(user);
        }

        public async Task<UserModel?> AuthenticateAsync(string username, string password)
        {
            var user = await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null || user.Password != password)
                return null;
            return user;
        }


    }
}
