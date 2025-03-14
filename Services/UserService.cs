#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class UserService
    {
        private readonly IMongoCollection<UserModel> _userCollection;

        public UserService(IMongoDatabase database)
        {
            this._userCollection = database.GetCollection<UserModel>("users");
        }

        public async Task<List<UserModel>> GetUsersAsync() => await this._userCollection.Find(p => !p.Status.Equals("deleted")).ToListAsync();

        public async Task CreateAsync(UserModel user, ObjectId logUserId)
        {
            user.Logs ??=[];
            await this._userCollection.InsertOneAsync(user);
        }

        public async Task<UserModel?> AuthenticateAsync(string username, string password)
        {
            var user = await this._userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null || user.Password != password)
            {
                return new UserModel();
            }

            return user;
        }
    }
}
