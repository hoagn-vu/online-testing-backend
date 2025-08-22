#pragma warning disable
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services;

public class UsersService
{
    private readonly UserRepository _userRepository;
    private readonly LogService _logService;

    public UsersService(UserRepository userRepository, LogService logService)
    {
        _userRepository = userRepository;
        _logService = logService;
    }

    //Get all User
    public async Task<(List<UserDto>, long)> GetAllUsers(string? keyword, int page, int pageSize, string? role)
    {
        var f = Builders<UsersModel>.Filter;
        var filter = f.Empty;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var safe = System.Text.RegularExpressions.Regex.Escape(keyword.Trim());
            var rx = new BsonRegularExpression(safe, "i");

            var keywordFilter = f.Or(
                f.Regex(u => u.FullName, rx),
                f.Regex(u => u.UserCode, rx)
            );

            filter = f.And(filter, keywordFilter);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleFilter = f.Eq(u => u.Role, role.Trim());
            filter = f.And(filter, roleFilter);
        }

        int skip = (page - 1) * pageSize;

        var users = await _userRepository.GetUsersAsync(filter, skip, pageSize);
        var total = await _userRepository.CountAsync(filter);

        return (users, total);
    }

    //Get user using ID
    public Task<UserDto?> GetUserByIdAsync(string id)
        => _userRepository.GetUserByIdAsync(id);

    //Get user by role
    public async Task<List<UserOptionsDto>> GetUsersByRole(string role)
    {
        var roleFilter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Role, role),
            Builders<UsersModel>.Filter.Ne(u => u.AccountStatus, "deleted"));

        var users = await _userRepository.GetUsersAsync(roleFilter, 0, int.MaxValue);

        return users.Select(u => new UserOptionsDto
        {
            UserId = u.Id,
            UserCode = u.UserCode,
            FullName = u.FullName
        }).ToList();
    }

    //Add new user
    public async Task<string> AddUser(CreateOrUpdateUserDto userData)
    {
        if (string.IsNullOrWhiteSpace(userData.UserName) ||
            string.IsNullOrWhiteSpace(userData.Password) ||
            string.IsNullOrWhiteSpace(userData.UserCode))
            return "Invalid user data";

        var existed = await _userRepository.GetByUsernameAsync(userData.UserName);
        if (existed != null) return "UserName already exists";

        var lastSpaceIndex = userData.FullName.LastIndexOf(' ');

        var user = new UsersModel
        {
            UserName = userData.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(userData.Password),
            UserCode = userData.UserCode,
            FullName = userData.FullName,
            LastName = userData.FullName[..lastSpaceIndex],
            FirstName = userData.FullName[(lastSpaceIndex + 1)..],
            Role = userData.Role,
            Gender = userData.Gender,
            DateOfBirth = userData.DateOfBirth,
            AccountStatus = !string.IsNullOrEmpty(userData.AccountStatus) ? userData.AccountStatus : "active",
            GroupName = userData.GroupName.Count > 0 ? userData.GroupName : [],
            Authenticate = userData.Authenticate.Count > 0 ? userData.Authenticate : []
        };

        try
        {
            await _userRepository.InsertAsync(user);

            return "User is added successfully";
        }
        catch (Exception ex)
        {
            return "Failed to create user" + ex;
        }

        //log handle
        //var userType = userData.Role switch
        //{
        //    "admin" => "quản trị viên: ",
        //    "staff" => "cán bộ phụ trách kỳ thi: ",
        //    "supervisor" => "giám thị: ",
        //    "candidate" => "thí sinh: ",
        //    _ => "tùy chỉnh: "
        //};

        //var logInsert = new LogsModel
        //{
        //    MadeBy = idMadeBy,
        //    LogAction = "create",
        //    LogDetails = "Tạo tài khoản " + userType + userData.UserName
        //};
    }

    //Update user by Id
    public async Task<string> UpdateUserById(string id, CreateOrUpdateUserDto updateUser)
    {
        var updateDef = new List<UpdateDefinition<UsersModel>>();
        var builder = Builders<UsersModel>.Update;

        if (!string.IsNullOrWhiteSpace(updateUser.UserName))
            updateDef.Add(builder.Set(x => x.UserName, updateUser.UserName));

        if (!string.IsNullOrWhiteSpace(updateUser.Password))
            updateDef.Add(builder.Set(x => x.Password, BCrypt.Net.BCrypt.HashPassword(updateUser.Password)));

        if (!string.IsNullOrWhiteSpace(updateUser.UserCode))
            updateDef.Add(builder.Set(x => x.UserCode, updateUser.UserCode));

        if (!string.IsNullOrWhiteSpace(updateUser.FullName))
        {
            var lastSpaceIndex = updateUser.FullName.LastIndexOf(' ');
            var lastName= updateUser.FullName[..lastSpaceIndex];
            var firstName = updateUser.FullName[(lastSpaceIndex + 1)..];

            updateDef.Add(builder.Set(x => x.FullName, updateUser.FullName));
            updateDef.Add(builder.Set(x => x.LastName, lastName));
            updateDef.Add(builder.Set(x => x.FirstName, firstName));
        }

        if (!string.IsNullOrWhiteSpace(updateUser.Role))
            updateDef.Add(builder.Set(x => x.Role, updateUser.Role));

        if (!string.IsNullOrWhiteSpace(updateUser.Gender))
            updateDef.Add(builder.Set(x => x.Gender, updateUser.Gender));

        if (!string.IsNullOrWhiteSpace(updateUser.DateOfBirth))
            updateDef.Add(builder.Set(x => x.DateOfBirth, updateUser.DateOfBirth));

        if (!string.IsNullOrWhiteSpace(updateUser.AccountStatus))
            updateDef.Add(builder.Set(x => x.AccountStatus, updateUser.AccountStatus));

        if (updateUser.GroupName is { Count: > 0 })
            updateDef.Add(builder.Set(x => x.GroupName, updateUser.GroupName));

        if (updateUser.Authenticate is { Count: > 0 })
            updateDef.Add(builder.Set(x => x.Authenticate, updateUser.Authenticate));

        if (!updateDef.Any())
            return "No valid fields to update";

        var update = builder.Combine(updateDef);

        try
        {
            var result = await _userRepository.UpdateAsync(id, update);

            if (result.ModifiedCount > 0)
                return "Success";

            return "Update user error";
        }
        catch (Exception ex)
        {
            return $"MongoDB Update Error: {ex.Message}";
        }
    }
    
    //Delete user by Id
    public async Task<string> DeleteUserById(string id, string madeBy)
    {
        try
        {
            // var result = await _userRepository.DeleteAsync(id);
            var update = Builders<UsersModel>.Update.Set(x => x.AccountStatus, "deleted");
            var result = await _userRepository.UpdateAsync(id, update);
            
            // if (result.DeletedCount > 0)
            // {
            //     //Log handle
            //     //await _logService.WriteAsync(new LogsModel
            //     //{
            //     //    MadeBy = madeBy,
            //     //    LogAction = "update",
            //     //    LogDetails = $"Cập nhật tài khoản {updateUser.UserName}"
            //     //});
            //     return "Success";
            // }
            
            return result.ModifiedCount > 0 ? "Success" : "Delete user error: No document delete";

            // return "Delete user error: No document delete";
        }
        catch (Exception ex)
        {
            return $"MongoDB Delete Error: {ex.Message}";
        }
    }

    //Get user review
    public async Task<ExamReviewDto> GetExamReviewAsync(string userId, string organizeExamId, string sessionId, string roomId)
    {
        var user = await _userRepository.FindUserAsync(userId) ?? throw new KeyNotFoundException("User not found");
        var takeExam = (user.TakeExam ?? new List<TakeExamsModel>())
            .FirstOrDefault(te => te.OrganizeExamId == organizeExamId &&
                                  te.SessionId == sessionId &&
                                  te.RoomId == roomId);
        var answers = takeExam?.Answers ?? new List<AnswersModel>();
        var totalScore = takeExam.TotalScore;
        var qIds = answers.Select(a => a.QuestionId).Distinct().ToHashSet();
        //Exam name - Exam Code
        //var exam = await _userRepository.FindExamAsync(examId)
        //OrganizeExam
        var organizeExam = await _userRepository.FindOrganizeExamAsync(organizeExamId);
        string organizeExamName = organizeExam.OrganizeExamName;
        int duration = organizeExam.Duration;

        var subjectId = organizeExam.SubjectId;
        var questionBankId = organizeExam.QuestionBankId;

        //Subject
        var subject = await _userRepository.FindSubjectAsync(subjectId) ?? throw new KeyNotFoundException("Subject not found");
        string subjectName = subject.SubjectName;
        var questionBank = (subject.QuestionBanks ?? new List<QuestionBanksModel>()).FirstOrDefault(
                qb => qb.QuestionBankId == questionBankId
            );
        string questionBankName = questionBank.QuestionBankName;

        var qb = (subject.QuestionBanks ?? new List<QuestionBanksModel>())
                .FirstOrDefault(x => x.QuestionBankId == questionBankId)
             ?? throw new KeyNotFoundException("Question bank not found in subject");

        var questionList = (qb.QuestionList ?? new List<QuestionModel>())
                       .Where(q => qIds.Contains(q.QuestionId))
                       .ToList();

        var questionDict = questionList.ToDictionary(q => q.QuestionId, q => q);

        var session = (organizeExam.Sessions ?? new List<SessionsModel>())
            .FirstOrDefault(s => s.SessionId == sessionId);
        string sessionName = session.SessionName;

        var room = await _userRepository.FindRoomAsync(roomId);
        string roomName = room.RoomName;


        var dto = new ExamReviewDto
        {
            FullName = user.FullName ?? string.Empty,
            OrganizeExamName = organizeExamName,
            SessionName = sessionName,
            RoomName = roomName,
            Duration = duration,
            SubjectName = subjectName,
            TotalScore = totalScore,
            QuestionBankName = questionBankName,
            Questions = answers.Select(a =>
            {
                questionDict.TryGetValue(a.QuestionId, out var q);
                return new QuestionReviewDto
                {
                    QuestionId = a.QuestionId,
                    QuestionText = q?.QuestionText ?? string.Empty,
                    Tags = q?.Tags ?? new List<string>(),
                    Options = (q?.Options ?? new List<OptionsModel>()).Select(op => new OptionsModel
                    {
                        OptionId = op.OptionId,
                        OptionText = op.OptionText ?? string.Empty,
                        IsCorrect = op.IsCorrect
                    }).ToList(),
                    AnswerChosen = a.AnswerChosen ?? new List<string>(),
                    IsUserChosenCorrect = a.IsCorrect
                };
            }).ToList()
        };

        return dto;
    }

    public async Task<ResumeExamResponse> ResumeAsync(string userId, string organizeExamId, string roomId, string sessionId)
    {
        var user = await _userRepository.FindUserAsync(userId)
            ?? throw new InvalidOperationException("User not found or deleted.");

        var organizeExam = await _userRepository.FindOrganizeExamAsync(organizeExamId)
            ?? throw new InvalidOperationException("OrganizeExam not found.");

        var subjectId = organizeExam.SubjectId
            ?? throw new InvalidOperationException("OrganizeExam missing SubjectId.");

        var questionBankId = organizeExam.QuestionBankId
            ?? throw new InvalidOperationException("OrganizeExam missing QuestionBankId.");

        var subejct = await _userRepository.FindSubjectAsync(subjectId);

        var room = await _userRepository.FindRoomAsync(roomId);

        var qb = await _userRepository.FindQuestionBankAsync(subjectId, questionBankId)
            ?? throw new InvalidOperationException("QuestionBank not found in Subject.");

        var questionList = qb.QuestionList ?? new List<QuestionModel>();

        var fullName = user.FullName ?? "";
        var userCode = user.UserCode ?? "";
        var organizeExamName = organizeExam.OrganizeExamName ?? "";
        var subjectName = subejct.SubjectName ?? "";
        var roomName = room.RoomName ?? "";

        user.TakeExam ??= new List<TakeExamsModel>();
        var take = user.TakeExam.FirstOrDefault(t =>
               t.OrganizeExamId == organizeExamId &&
               t.SessionId == sessionId &&
               t.RoomId == roomId
         );

        //If not exist take Exam
        if (take is null)
        {
            take = new TakeExamsModel
            {
                Id = Guid.NewGuid().ToString("N"),
                OrganizeExamId = organizeExamId,
                SessionId = sessionId,
                RoomId = roomId,
                Status = "in_progress",
                StartAt = DateTime.UtcNow,
                Progress = 0,
                Answers = new List<AnswersModel>()
            };

            // push 
            var updatePush = Builders<UsersModel>.Update.Push(u => u.TakeExam, take);
            await _userRepository.UpdateAsync(user.Id, updatePush);
        }

        //Map answers according to QuestionId
        var answerByQid = take.Answers?.ToDictionary(a => a.QuestionId, a => a)
                          ?? new Dictionary<string, AnswersModel>();

        //Merge question
        var merged = new List<ResumeQuestionItem>();
        foreach (var q in questionList)
        {
            var opts = (q.Options ?? new List<OptionsModel>())
                .Select(o => new QuestionOptionItem { OptionId = o.OptionId, OptionText = o.OptionText })
                .ToList();

            if (answerByQid.TryGetValue(q.QuestionId, out var ans))
            {
                merged.Add(new ResumeQuestionItem
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Options = opts,
                    SelectedOptionIds = ans.AnswerChosen,
                    IsCorrect = ans.IsCorrect,
                    Tags = q.Tags ?? new List<string>()
                });
            }
            //else
            //{
            //    merged.Add(new ResumeQuestionItem
            //    {
            //        QuestionId = q.QuestionId,
            //        QuestionText = q.QuestionText,
            //        QuestionType = q.QuestionType,
            //        Options = opts,
            //        SelectedOptionIds = Array.Empty<string>(),
            //        IsCorrect = null,
            //        Tags = q.Tags ?? new List<string>()
            //    });
            //}
        }

        var takeProgress = take.Progress;

        return new ResumeExamResponse
        {
            UserId = userId,
            FullName = fullName,
            UserCode = userCode,
            OrganizeExamId = organizeExamId,
            OrganizeExamName = organizeExamName,
            SessionId = sessionId,
            RoomId = roomId,
            RoomName = roomName,
            SubjectId = subjectId,
            SubjectName = subjectName,
            QuestionBankId = questionBankId,
            Status = take.Status,
            Progress = takeProgress,
            TotalQuestions = merged.Count,
            Questions = merged
        };
    }
}
