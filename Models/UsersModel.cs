namespace Backend_online_testing.Models
{
    using System.ComponentModel.DataAnnotations;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class UsersModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userName")]
        required public string UserName { get; set; }

        [BsonElement("userCode")]
        required public string UserCode { get; set; }

        [BsonElement("password")]
        required public string Password { get; set; }

        [BsonElement("fullName")]
        required public string FullName { get; set; }

        [BsonElement("role")]
        public string? Role { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [BsonElement("groupName")]
        public List<string> GroupName { get; set; } = new List<string>();

        [BsonElement("accountStatus")]
        required public string AccountStatus { get; set; }

        [BsonElement("authenticate")]
        public List<string>? Authenticate { get; set; }

        [BsonElement("userLogs")]
        public List<UserLogsModel>? UserLog { get; set; } = new List<UserLogsModel>();
    }
}
