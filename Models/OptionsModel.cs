﻿namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class OptionsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OptionId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("optionText")]
        public string OptionText { get; set; } = string.Empty;

        [BsonElement("isCorrect")]
        public bool? IsCorrect { get; set; }
    }
}
