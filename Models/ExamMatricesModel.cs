﻿namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class ExamMatricesModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("matrixName")]
        public string MatrixName { get; set; } = string.Empty;

        [BsonElement("matrixStatus")]
        public string MatrixStatus { get; set; } = "available";

        [BsonElement("subjectId")]
        public string SubjectId { get; set; } = string.Empty;

        [BsonElement("questionBankId")]
        public string QuestionBankId { get; set; } = string.Empty;

        [BsonElement("matrixTags")]
        public List<MatrixTagsModel> MatrixTags { get; set; } = [];

        [BsonElement("examIds")]
        public List<string> ExamIds { get; set; } = [];
    }
}
