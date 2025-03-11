using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace backend_online_testing.Models
{
    public class UsersModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("userCode")]
        public string UserCode { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("fullName")]
        public string FullName { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [BsonElement("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [BsonElement("groupName")]
        public List<string>? GroupName { get; set; }

        [BsonElement("accountStatus")]
        public string AccountStatus { get; set; }

        [BsonElement("authenticate")]
        public List<string>? Authenticate { get; set; }

        [BsonElement("userLogs")]
        public List<UserLogsModel>? UserLog { get; set; }
    }
}
