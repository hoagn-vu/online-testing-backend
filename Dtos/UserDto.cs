namespace Backend_online_testing.Dtos;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public List<string> GroupName { get; set; } = [];
    public string AccountStatus { get; set; } = string.Empty;
    public List<string> Authenticate { get; set; } = [];
}

public class CreateOrUpdateUserDto
{
    public string? UserName { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? UserCode { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? Role { get; set; } = string.Empty;
    public string? Gender { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; } = string.Empty;
    public List<string>? GroupName { get; set; } = [];
    public string? AccountStatus { get; set; } = string.Empty;
    public List<string>? Authenticate { get; set; } = [];
}

public class UserOptionsDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class ResumeExamResponse
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string UserCode { get; set; } = string.Empty;

    public string OrganizeExamId { get; set; } = string.Empty;

    public string OrganizeExamName { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string RoomId { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public string SubjectId { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string QuestionBankId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Progress { get; set; } 

    public int TotalQuestions { get; set; }

    public List<ResumeQuestionItem> Questions { get; set; } = new();
}

public class ResumeQuestionItem
{
    public string QuestionId { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;
    public IReadOnlyList<QuestionOptionItem> Options { get; set; } = Array.Empty<QuestionOptionItem>();
    public IReadOnlyList<string> SelectedOptionIds { get; set; } = Array.Empty<string>(); 
    public bool IsAnswered => SelectedOptionIds.Count > 0;
    public bool? IsCorrect { get; set; } 
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}

public class QuestionOptionItem
{
    public string OptionId { get; set; } = default!;
    public string OptionText { get; set; } = default!;
}

public class BulkChangePasswordRequestDto
{
    public List<string> UserIds { get; set; } = new();
    public required string NewPassword { get; set; }
}

public class ChangePasswordRequestDto
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}

public class UpdateTakeExamRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string OrganizeExamId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // reopen | restart
    public string? Status { get; set; }
}

public class UpdateSessionPasswordRequestDto
{
    public string OrganizeExamId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateSessionPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UpdatedUserCount { get; set; }
}
