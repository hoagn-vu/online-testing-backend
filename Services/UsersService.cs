#pragma warning disable
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
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
        //FilterDefinition<UsersModel>? keywordFilter = null;
        //if (!string.IsNullOrWhiteSpace(keyword))
        //{
        //    var regex = new BsonRegularExpression(keyword, "i");
        //    keywordFilter = Builders<UsersModel>.Filter.Or(
        //        Builders<UsersModel>.Filter.Regex(u => u.FullName, regex),
        //        Builders<UsersModel>.Filter.Regex(u => u.UserCode, regex));
        //}
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
    public async Task<string> AddUser(UsersModel userData)
    {
        //Check enter data
        if (string.IsNullOrWhiteSpace(userData.UserName) ||
            string.IsNullOrWhiteSpace(userData.Password) ||
            string.IsNullOrWhiteSpace(userData.UserCode))
            return "Invalid user data";

        //Check if username exist
        var existed = await _userRepository.GetByUsernameAsync(userData.UserName);
        if (existed != null) return "UserName already exists";

        if (string.IsNullOrWhiteSpace(userData.Id))
            userData.Id = ObjectId.GenerateNewId().ToString();

        userData.Password = BCrypt.Net.BCrypt.HashPassword(userData.Password);

        //Add user to database
        try
        {
            await _userRepository.InsertAsync(userData);

            return "User is added successfully";
        }
        catch (Exception ex)
        {
            return "Failed to create user"+ex;
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
    public async Task<string> UpdateUserById(string id, UsersModel updateUser, string madeBy)
    {
        if (!string.IsNullOrWhiteSpace(updateUser.Password))
            updateUser.Password = BCrypt.Net.BCrypt.HashPassword(updateUser.Password);

        var update = Builders<UsersModel>.Update
            .Set(x => x.FullName, updateUser.FullName)
            .Set(x => x.Password, updateUser.Password)
            .Set(x => x.Role, updateUser.Role)
            .Set(x => x.Gender, updateUser.Gender)
            .Set(x => x.DateOfBirth, updateUser.DateOfBirth)
            .Set(x => x.AccountStatus, updateUser.AccountStatus)
            .Set(x => x.GroupName, updateUser.GroupName);

        try
        {
            var result = await _userRepository.UpdateAsync(id, update);

            if (result.ModifiedCount > 0)
            {
                //Log handle
                //await _logService.WriteAsync(new LogsModel
                //{
                //    MadeBy = madeBy,
                //    LogAction = "update",
                //    LogDetails = $"Cập nhật tài khoản {updateUser.UserName}"
                //});
                return "Success";
            }

            return "Update user error: No document updated (possibly not found or data unchanged)";
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
            var result = await _userRepository.DeleteAsync(id);

            if (result.DeletedCount > 0)
            {
                //Log handle
                //await _logService.WriteAsync(new LogsModel
                //{
                //    MadeBy = madeBy,
                //    LogAction = "update",
                //    LogDetails = $"Cập nhật tài khoản {updateUser.UserName}"
                //});
                return "Success";
            }

            return "Delete user error: No document delete";
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
}
