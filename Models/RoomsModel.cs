﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class RoomsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("roomStatus")]
        public string? RoomStatus { get; set; } = "available";

        [BsonElement("roomName")]
        public string? RoomName { get; set; } = string.Empty;

        [BsonElement("roomCapacity")]
        public int? RoomCapacity { get; set; }

        [BsonElement("roomLocation")]
        public string? RoomLocation { get; set; }
    }
}
