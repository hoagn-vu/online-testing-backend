﻿namespace Backend_online_testing.DTO
{
    using Backend_online_testing.Models;

    public class ExamDTO
    {
        public string? Id { get; set; }

        public string? ExamCode { get; set; }

        public string? ExamName { get; set; }

        public string? SubjectId { get; set; }

        public string? ExamStatus { get; set; }

        public string? ExamLogUserId { get; set; }
    }
}
