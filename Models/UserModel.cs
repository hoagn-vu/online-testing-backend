using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend_online_testing.Models
{
    public class UserModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("userName")]
        public required string Username { get; set; }
        [BsonElement("password")]
        public required string Password { get; set; }
        [BsonElement("fullName")]
        public string? FullName { get; set; }
        [BsonElement("dateOfBirth")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? Dob { get; set; }
        [BsonElement("gender")]
        public string? Gender { get; set; }
        [BsonElement("userCode")]
        public string? UserCode { get; set; }
        [BsonElement("role")]
        public string? Role { get; set; }
        [BsonElement("authenticate")]
        public List<string>? Auth { get; set; }
        [BsonElement("groupName")]
        public List<string>? Groups { get; set; }
        [BsonElement("accountStatus")]
        public string Status { get; set; } = "available";
        [BsonElement("accountLogs")]
        public List<UserLogsModel>? Logs { get; set; }
    }
}
