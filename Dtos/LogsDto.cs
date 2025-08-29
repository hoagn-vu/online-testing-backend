namespace Backend_online_testing.Dtos
{
    public class LogsDto
    {
        
    }
    
    public class CreateLogDto
    {
        public string MadeBy { get; set; } = string.Empty;
        public string LogAction { get; set; } = string.Empty;
        public string LogDetails { get; set; } = string.Empty;
    }

    public class LogResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string MadeBy { get; set; } = string.Empty;
        public string LogAction { get; set; } = string.Empty;
        public DateTime LogAt { get; set; }
        public string LogDetails { get; set; } = string.Empty;
    }
}