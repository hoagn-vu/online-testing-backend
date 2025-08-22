#pragma warning disable SA1309
namespace Backend_online_testing.Controllers;

using System.Runtime.CompilerServices;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/subjects")]
[ApiController]
public class SubjectsController : ControllerBase
{
    private readonly SubjectsService _subjectsService;
    private readonly S3Service _s3Service;

    public SubjectsController(SubjectsService subjectsService, S3Service s3Service)
    {
        _subjectsService = subjectsService;
        _s3Service = s3Service;
    }

    // Get subject
    [HttpGet]
    public async Task<IActionResult> GetSubjects([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (subjects, totalCount) = await _subjectsService.GetSubjects(keyword ?? string.Empty, page, pageSize);

        return Ok(new { subjects, totalCount });
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetSubjectOptions()
    {
        var subjects = await this._subjectsService.GetAllSubjects();

        return Ok(subjects);
    }

    // Add subject
    [HttpPost]
    public async Task<ActionResult> AddSubject([FromBody] SubjectRequestDto? subjectDto)
    {
        if (subjectDto == null || string.IsNullOrEmpty(subjectDto.SubjectName))
        {
            return BadRequest(new { error = "Subject name is required" });
        }

        var result = await this._subjectsService.AddSubject(subjectDto.SubjectName);

        if (result == "Add subject successfully")
        {
            return Ok(new { message = result });
        }
        else
        {
            return BadRequest(new { error = result });
        }
    }

    // Update Subject
    [HttpPut("update/{subjectId}")]
    public async Task<ActionResult> UpdateSubject(string subjectId, [FromBody] SubjectRequestDto? subjectDto)
    {
        if (subjectDto == null || string.IsNullOrEmpty(subjectDto.SubjectName))
        {
            return BadRequest(new { error = "Subject is required" });
        }

        var result = await _subjectsService.UpdateSubject(subjectId, subjectDto.SubjectName);

        if (result == "Update subject successfully")
        {
            return Ok(new { message = result });
        }
        else
        {
            return BadRequest(new { error = result });
        }
    }

    // Search by question bank name
    [HttpGet("{subjectId}/question-banks")]
    public async Task<ActionResult<QuestionBankPerSubjectDto?>> GetQuestionBanks([FromRoute]string subjectId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var questionBanks = await _subjectsService.GetQuestionBanks(subjectId, keyword, page, pageSize);
        if (questionBanks == null) return NotFound();
        
        return Ok(questionBanks);
    }

    [HttpGet("question-bank-options")]
    public async Task<ActionResult<List<SubjectsModel>>> GetQuestionBankOptions([FromQuery] string subjectId)
    {
        var questionBanks = await _subjectsService.GetQuestionBanksPerSubject(subjectId);
        return Ok(questionBanks);
    }

    // Add question bank
    [HttpPost("question-bank")]
    public async Task<ActionResult> AddQuestionBankName([FromBody] QuestionBankRequestDto? questionBankDto)
    {
        if (questionBankDto == null) return BadRequest(new { error = "Question bank is required" });
        var result = await this._subjectsService.AddQuestionBank(questionBankDto.SubjectId, questionBankDto.QuestionBankName);

        if (result == "Add question bank successfully")
        {
            return this.Ok(new { message = result });
        }
        else
        {
            return this.BadRequest(new { error = result });
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateQuestionBankName([FromBody] QuestionBankRequestDto? questionBankDto)
    {
        if (questionBankDto == null) return BadRequest(new { error = "Question bank is required" });
        var result = await _subjectsService.UpdateQuestionBankName(questionBankDto.SubjectId, questionBankDto.QuestionBankId ?? string.Empty, questionBankDto.QuestionBankName);

        if (result == "Update question bank name successfully")
        {
            return Ok(new { message = result });
        }
        else
        {
            return this.BadRequest(new { error = result });
        }
    }

    // Search by question name
    [HttpGet("questions")]
    public async Task<ActionResult<List<SubjectsModel>>> GetQuestions(string subId, string qbId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (subjectId, subjectName, questionBankId, questionBankName, allChapter, allLevel, questions, totalCount) = await _subjectsService.GetQuestions(subId, qbId, keyword, page, pageSize);
        return Ok(new { subjectId, subjectName, questionBankId, questionBankName, allChapter, allLevel, questions, totalCount });
    }

    // Add question list
    [HttpPost("add-question")]
    public async Task<ActionResult> AddQuestion(
        [FromQuery] string subjectId, 
        [FromQuery] string questionBankId, 
        [FromBody] SubjectQuestionDto question,
        [FromForm] List<IFormFile>? images
    )
    {
        var uploadedImageUrls = new List<string>();

        if (images is not null)
        {
            foreach (var image in images)
            {
                var url = await _s3Service.UploadFileAsync(image);
                uploadedImageUrls.Add(url);
            }
        }

        // Gán đường dẫn ảnh vào câu hỏi
        question.ImgLinks = uploadedImageUrls;

        var result = await _subjectsService.AddQuestion(subjectId, questionBankId, question);

        if (result == "Thêm câu hỏi thành công")
        {
            return this.Ok(new { message = result });
        }
        else
        {
            return this.BadRequest(new { error = result });
        }
    }
    
    [HttpPost("new-add-question")]
    public async Task<ActionResult> NewAddQuestion(
        [FromQuery] string subjectId,
        [FromQuery] string questionBankId,
        [FromForm] AddQuestionDto request
    )
    {
        var uploadedImageUrls = new List<string>();

        if (request.Images is not null)
        {
            foreach (var image in request.Images)
            {
                var url = await _s3Service.UploadFileAsync(image);
                uploadedImageUrls.Add(url);
            }
        }

        // Gán đường dẫn ảnh vào câu hỏi
        request.Question.ImgLinks = uploadedImageUrls;

        var result = await _subjectsService.AddQuestion(subjectId, questionBankId, request.Question);

        if (result == "Thêm câu hỏi thành công")
        {
            return this.Ok(new { message = result });
        }

        return BadRequest(new { error = result });
    }
    
    [HttpPost("questions")]
    public async Task<ActionResult> AddQuestions([FromQuery] string subjectId, [FromQuery] string questionBankId, [FromBody] List<SubjectQuestionDto> questions)
    {
        var result = await _subjectsService.AddQuestions(subjectId, questionBankId, questions);

        if (result == "ok")
        {
            return Ok(new { message = result });
        }
        else
        {
            return BadRequest(new { error = result });
        }
    }

    // Update question Id
    [HttpPut("update-question/{questionId}")]
    public async Task<ActionResult> UpdateQuestion(string questionId, [FromQuery] string subjectId, [FromQuery] string questionBankId, [FromQuery] string userId, [FromBody] SubjectQuestionDto questionData)
    {
        var result = await _subjectsService.UpdateQuestion(subjectId, questionBankId, questionId, userId, questionData);

        if (result == "Update question successfully")
        {
            return Ok(new { message = result });
        }
        else
        {
            return BadRequest(new { error = result });
        }
    }

    // Delete Subject
    [HttpDelete("delete-subject")]
    public async Task<ActionResult> DeleteSubject(string subjectId)
    {
        var result = await this._subjectsService.DeleteSubject(subjectId);

        if (result == "Delete subject successfully")
        {
            return this.Ok(new { message = result });
        }
        else
        {
            return this.BadRequest(new { message = result });
        }
    }

    // Delete question bank
    [HttpDelete("delete-question-bank")]
    public async Task<ActionResult> DeleteQuestionBank(string subjectId, string questionBankId)
    {
        var result = await this._subjectsService.DeleteQuestionBank(subjectId, questionBankId);

        if (result == "Delete question bank successfully")
        {
            return this.Ok(new { message = result });
        }
        else
        {
            return this.BadRequest(new { message = result });
        }
    }

    // Delete question
    [HttpDelete("delete/question")]
    public async Task<ActionResult> DeleteQuestion(string subjectId, string questionBankId, string questionId, string userLogId)
    {
        var result = await this._subjectsService.DeleteQuestion(subjectId, questionBankId, questionId, userLogId);

        if (result == "Question deleted successfully")
        {
            return this.Ok(new { message = result });
        }
        else
        {
            return this.BadRequest(new { message = result });
        }
    }

    [HttpGet("questions/tags-classification")]
    public async Task<ActionResult<List<TagsClassification>>> GetTagsClassification([FromQuery] string subjectId, [FromQuery] string questionBankId, [FromQuery] string type)
    {
        var result = await _subjectsService.GetTagsClassificationAsync(subjectId, questionBankId, type);
        return Ok(result);
    }
}