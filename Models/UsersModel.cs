﻿namespace Backend_online_testing.Models
{
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson;

    public class UsersModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userName")]
        public required string UserName { get; set; }

        [BsonElement("userCode")]
        public required string UserCode { get; set; }

        [BsonElement("password")]
        public required string Password { get; set; }

        [BsonElement("fullName")]
        public required string FullName { get; set; }

        [BsonElement("role")]
        public string? Role { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [BsonElement("groupName")]
        public List<string> GroupName { get; set; } = [];

        [BsonElement("accountStatus")] 
        public string AccountStatus { get; set; } = "active";

        [BsonElement("authenticate")]
        public List<string>? Authenticate { get; set; }
        
        [BsonElement("refreshToken")]
        public string? RefreshToken { get; set; }
        
        [BsonElement("tokenExpiration")]
        public DateTime? TokenExpiration { get; set; }
        
        [BsonElement("trackExams")]
        public List<TrackExamsModel>? TrackExam { get; set; }

        [BsonElement("takeExams")] 
        public List<TakeExamsModel>? TakeExam { get; set; } = [];
    }
}
