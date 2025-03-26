#pragma warning disable
using System.Runtime.InteropServices.JavaScript;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<UsersModel> _users;
        private readonly AddLogService _logService;

        public UsersService(IMongoDatabase database, AddLogService logService)
        {
            _users = database.GetCollection<UsersModel>("users");
            _logService = logService;
        }

        //Get all User
        public async Task<(List<UserDto>, long)> GetAllUsers(string? keyword, int page, int pageSize)
        {
            var filter = Builders<UsersModel>.Filter.Ne(us => us.AccountStatus, "deleted");

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<UsersModel>.Filter.Or(
                    Builders<UsersModel>.Filter.Regex(u => u.FullName, new BsonRegularExpression(keyword, "i")),
                    Builders<UsersModel>.Filter.Regex(u => u.UserCode, new BsonRegularExpression(keyword, "i"))
                );
            }
            
            var projection = Builders<UsersModel>.Projection
                .Expression(u => new UserDto
                {
                    Id = u.Id.ToString(), // Chuyển đổi ObjectId thành string
                    Username = u.UserName,
                    FullName = u.FullName,
                    Role = u.Role ?? string.Empty,
                    UserCode = u.UserCode,
                    Gender = u.Gender ?? string.Empty,
                    DateOfBirth = u.DateOfBirth ?? string.Empty,
                    GroupName = u.GroupName ?? new List<string>(),
                    Authenticate = u.Authenticate ?? new List<string>(),
                    AccountStatus = u.AccountStatus
                });

            var users = await _users
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(projection)
                .ToListAsync();

            var totalRecords = await _users.CountDocumentsAsync(filter);

            return (users, totalRecords);
        }

        //Get user using ID
        public async Task<UsersModel?> GetUserById(string id)
        {
            var filter = Builders<UsersModel>.Filter.Eq(x => x.Id, id);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<UserOptionsDto>> GetUsersByRole(string role)
        {
            var filter = Builders<UsersModel>.Filter.And(
                Builders<UsersModel>.Filter.Eq(s => s.Role, role),
                Builders<UsersModel>.Filter.Eq(s => s.AccountStatus, "active")
            );
            
            var projection = Builders<UsersModel>.Projection
                .Expression(sub => new UserOptionsDto
                {
                    UserId = sub.Id.ToString(),
                    UserCode = sub.UserCode,
                    FullName = sub.FullName,
                });
            
            return await _users.Find(filter).Project(projection).ToListAsync();
        }

        //Add new user
        public async Task<string> AddUser(UsersModel userData, string idMadeBy)
        {
            //Check enter data
            if (string.IsNullOrWhiteSpace(userData.UserName) || string.IsNullOrWhiteSpace(userData.Password) || string.IsNullOrWhiteSpace(userData.UserCode) || string.IsNullOrWhiteSpace(idMadeBy))
            {
                return "Invalid user data";
            }

            var existingUser = await _users.Find(u => u.UserName == userData.UserName).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return "UserName already exists";
            }

            if (string.IsNullOrWhiteSpace(userData.Id))
            {
                userData.Id = ObjectId.GenerateNewId().ToString();
            }

            var userType = userData.Role switch
            {
                "admin" => "quản trị viên: ",
                "staff" => "cán bộ phụ trách kỳ thi: ",
                "supervisor" => "giám thị: ",
                "candidate" => "thí sinh: ",
                _ => "tùy chỉnh: "
            };

            var adminUser = await _users.Find(u => u.Id == idMadeBy).FirstOrDefaultAsync();
            if (adminUser != null)
            {
                adminUser.UserLog ??= [];

                adminUser.UserLog.Add(new UserLogsModel
                {
                    LogAction = "create",
                    LogDetails = "Tạo tài khoản " + userType + userData.UserName
                });
            }
            
            try
            {
                userData.Password = BCrypt.Net.BCrypt.HashPassword(userData.Password);
                await _users.InsertOneAsync(userData);
                
                var update = Builders<UsersModel>.Update.Set(u => u.UserLog, adminUser.UserLog);
                await _users.UpdateOneAsync(u => u.Id == idMadeBy, update);
                
                return "User is added successfully";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //Update user by Id
        public async Task<bool> UpdateUserbByID(string id, UsersModel updateUser)
        {
            var filter = Builders<UsersModel>.Filter.Eq(x => x.Id, id);
            var update = Builders<UsersModel>.Update
                .Set(x => x.FullName, updateUser.FullName)
                .Set(x => x.Password, updateUser.Password)
                .Set(x => x.Role, updateUser.Role)
                .Set(x => x.Gender, updateUser.Gender)
                .Set(x => x.DateOfBirth, updateUser.DateOfBirth)
                .Set(x => x.AccountStatus, updateUser.AccountStatus)
                .Set(x => x.GroupName, updateUser.GroupName);

            UserLogsModel lastLog = updateUser.UserLog != null && updateUser.UserLog.Any()
            ? new UserLogsModel
            {
                LogId = ObjectId.GenerateNewId().ToString(),
                LogAction = updateUser.UserLog.Last().LogAction,
                LogAt = updateUser.UserLog.Last().LogAt,
                LogDetails = updateUser.UserLog.Last().LogDetails,
                AffectedObject = updateUser.UserLog.Last().AffectedObject,
                AffectedObjectId = updateUser.UserLog.Last().AffectedObjectId
            }
            : new UserLogsModel
            {
                LogId = ObjectId.GenerateNewId().ToString(),
                LogAction = "Update",
                LogDetails = $"{updateUser.FullName} updated",
                AffectedObject = "",
                AffectedObjectId = ""
            };

            Console.WriteLine(lastLog);

            var updateLog = Builders<UsersModel>.Update.Push(x => x.UserLog, lastLog);

            var result = await _users.UpdateOneAsync(filter, Builders<UsersModel>.Update.Combine(update, updateLog));
            return result.ModifiedCount > 0;
        }
        //Delete user by Id
        public async Task<bool> DeleteUserById(string id)
        {
            var filter = Builders<UsersModel>.Filter.Eq(x => x.Id, id);
            var result = await _users.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        //Insert sample data
        public async Task InsertSampleData()
        {
            var sampleUsers = new List<UsersModel>
            {
                //new UsersModel {UserName = "user1", Password = "password1", Role = "Admin", Gender = "Male", PhoneNumber = "1234567890", DateOfBirth = "1990-01-01" },
                //new UsersModel {UserName = "user2", Password = "password2", Role = "User", Gender = "Female", PhoneNumber = "1234567891", DateOfBirth = "1992-02-02" },
                //new UsersModel {UserName = "user3", Password = "password3", Role = "User", Gender = "Male", PhoneNumber = "1234567892", DateOfBirth = "1994-03-03" },
                //new UsersModel {UserName = "user4", Password = "password4", Role = "User", Gender = "Female", PhoneNumber = "1234567893", DateOfBirth = "1996-04-04" },
                //new UsersModel {UserName = "user5", Password = "password5", Role = "Admin", Gender = "Male", PhoneNumber = "1234567894", DateOfBirth = "1998-05-05" },
                //new UsersModel {UserName = "user5", Password = "password5", Role = "Admin", Gender = "Male", PhoneNumber = "1234567894", DateOfBirth = "1998-05-05" },
                //new UsersModel {UserName = "user5", Password = "password5", Role = "Admin", Gender = "Male", PhoneNumber = "1234567894", DateOfBirth = "1998-05-05" },
                //new UsersModel {UserName = "user6", Password = "password6", Role = "User", Gender = "Male", PhoneNumber = "1234567895", DateOfBirth = "2000-06-06" },
                //new UsersModel {UserName = "user7", Password = "password7", Role = "User", Gender = "Female", PhoneNumber = "1234567896", DateOfBirth = "2002-07-07" },
                //new UsersModel {UserName = "user8", Password = "password8", Role = "User", Gender = "Male", PhoneNumber = "1234567897", DateOfBirth = "2004-08-08" },
                //new UsersModel {UserName = "user9", Password = "password9", Role = "Admin", Gender = "Female", PhoneNumber = "1234567898", DateOfBirth = "2006-09-09" },
                //new UsersModel {UserName = "user10", Password = "password10", Role = "User", Gender = "Male", PhoneNumber = "1234567899", DateOfBirth = "2008-10-10" }
            };

            await _users.InsertManyAsync(sampleUsers);
        }
    }
}
