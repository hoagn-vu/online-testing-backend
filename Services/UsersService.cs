using backend_online_testing.Models;
using MongoDB.Driver;

namespace backend_online_testing.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<UsersModel>? _users;

        public UsersService(IMongoDatabase database)
        {
            _users = database.GetCollection<UsersModel>("Users");
        }
        
        //Get all User
        public async Task<IEnumerable<UsersModel>> GetAllUsers()
        {
            return await _users.Find(FilterDefinition<UsersModel>.Empty).ToListAsync();
        }
        
        //Get user using ID
        public async Task<UsersModel?> GetUserById(string id)
        {
            var filter = Builders<UsersModel>.Filter.Eq(x => x.Id, id);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }
        //Update user by Id
        public async Task<bool> UpdateUserbByID(string id, UsersModel updateUser)
        {
            var filter = Builders<UsersModel>.Filter.Eq(x=>x.Id, id);
            var update = Builders<UsersModel>.Update
                .Set(x => x.UserName, updateUser.UserName)
                .Set(x => x.Password, updateUser.Password)
                .Set(x => x.Role, updateUser.Role)
                .Set(x => x.Email, updateUser.Email)
                .Set(x => x.Gender, updateUser.Gender)
                .Set(x => x.PhoneNumber, updateUser.PhoneNumber)
                .Set(x => x.DateOfBirth, updateUser.DateOfBirth);
        
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        //Delete user by Id
        public async Task<bool> DeleteUserById(string id)
        {
            var filter = Builders<UsersModel>.Filter.Eq(x=>x.Id, id);
            var result = await _users.DeleteOneAsync(filter);
            
            return result.DeletedCount > 0;
        }

        //Insert sample data
        public async Task InsertSampleData()
        {
            var sampleUsers = new List<UsersModel>
            {
                new UsersModel {Id="1", UserName = "user1", Password = "password1", Role = "Admin", Email = "user1@example.com", Gender = "Male", PhoneNumber = "1234567890", DateOfBirth = "1990-01-01" },
                new UsersModel {Id="2", UserName = "user2", Password = "password2", Role = "User", Email = "user2@example.com", Gender = "Female", PhoneNumber = "1234567891", DateOfBirth = "1992-02-02" },
                new UsersModel {Id="3", UserName = "user3", Password = "password3", Role = "User", Email = "user3@example.com", Gender = "Male", PhoneNumber = "1234567892", DateOfBirth = "1994-03-03" },
                new UsersModel {Id="4", UserName = "user4", Password = "password4", Role = "User", Email = "user4@example.com", Gender = "Female", PhoneNumber = "1234567893", DateOfBirth = "1996-04-04" },
                new UsersModel {Id="5", UserName = "user5", Password = "password5", Role = "Admin", Email = "user5@example.com", Gender = "Male", PhoneNumber = "1234567894", DateOfBirth = "1998-05-05" },
                new UsersModel {Id="6", UserName = "user5", Password = "password5", Role = "Admin", Email = "user5@example.com", Gender = "Male", PhoneNumber = "1234567894", DateOfBirth = "1998-05-05" },
                new UsersModel {Id="7", UserName = "user5", Password = "password5", Role = "Admin", Email = "user5@example.com", Gender = "Male", PhoneNumber = "1234567894", DateOfBirth = "1998-05-05" },
                new UsersModel {Id="8", UserName = "user6", Password = "password6", Role = "User", Email = "user6@example.com", Gender = "Male", PhoneNumber = "1234567895", DateOfBirth = "2000-06-06" },
                new UsersModel {Id="9", UserName = "user7", Password = "password7", Role = "User", Email = "user7@example.com", Gender = "Female", PhoneNumber = "1234567896", DateOfBirth = "2002-07-07" },
                new UsersModel {Id="10", UserName = "user8", Password = "password8", Role = "User", Email = "user8@example.com", Gender = "Male", PhoneNumber = "1234567897", DateOfBirth = "2004-08-08" },
                new UsersModel {Id="12", UserName = "user9", Password = "password9", Role = "Admin", Email = "user9@example.com", Gender = "Female", PhoneNumber = "1234567898", DateOfBirth = "2006-09-09" },
                new UsersModel {Id="13", UserName = "user10", Password = "password10", Role = "User", Email = "user10@example.com", Gender = "Male", PhoneNumber = "1234567899", DateOfBirth = "2008-10-10" }
            };

            await _users.InsertManyAsync(sampleUsers);
        }
    }
}
