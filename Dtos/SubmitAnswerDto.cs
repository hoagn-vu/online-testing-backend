namespace Backend_online_testing.Dtos
{
    public class SubmitAnswerDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public List<string> OptionIds { get; set; } = [];
    }
}