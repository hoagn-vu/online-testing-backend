﻿#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/exam-matrix")]
    [ApiController]
    public class ExamMatrixsController : ControllerBase
    {
        private readonly ExamMatrixsService _examMatrixsService;

        public ExamMatrixsController(ExamMatrixsService examMatrixsService)
        {
            this._examMatrixsService = examMatrixsService;
        }

        // Get all exam matrix
        [HttpGet]
        public async Task<IActionResult> GetAllExamsMatrix()
        {
            var examMatrixs_list = await this._examMatrixsService.GetAllExamMatrix();
            if (examMatrixs_list == null || !examMatrixs_list.Any())
            {
                return this.NotFound(new { message = "No exam founds" });
            }

            return this.Ok(new { status = "Success", data = examMatrixs_list });
        }

        // Get by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdExamMatrix(string id)
        {
            var result = await this._examMatrixsService.GetByIdExamMatrix(id);

            if (result == null)
            {
                return this.NotFound(new { message = "Exam matrix not found" });
            }

            return this.Ok(new { status = "Success", data = result });
        }

        // Searchby name
        [HttpGet("search-name")]
        public async Task<IActionResult> SearchByName(string name)
        {
            var results = await this._examMatrixsService.SearchByName(name);

            if (results == null || !results.Any())
            {
                return this.NotFound(new { message = "No exam matrix found" });
            }

            return this.Ok(new { status = "Success", data = results });
        }

        // Add exam matrix
        [HttpPost]
        public async Task<IActionResult> AddExamMatrix([FromBody] ExamMatrixsModel examMatrixData, string matrixLogUserId)
        {
            if (examMatrixData == null)
            {
                return this.BadRequest(new { message = "Invalid exam matrix data" });
            }

            var result = await this._examMatrixsService.AddExamMatrix(examMatrixData, matrixLogUserId);

            if (result == "Exam matrix created successfully")
            {
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        // Add tag
        [HttpPost("tags/")]
        public async Task<IActionResult> AddTag(string examMatrixId, [FromBody] ExamMatrixAddDto tagsData)
        {
            var result = await this._examMatrixsService.AddTag(tagsData);
            if (result == "Tags added successfully")
            {
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        // Update exam matrix
        [HttpPost("{examMatrixId}")]
        public async Task<IActionResult> UpdateMatrixExam(string examMatrixId, [FromBody] ExamMatrixUpdateDto examMatrixData)
        {
            var result = await this._examMatrixsService.UpdateExamMatrix(examMatrixId, examMatrixData);
            if (result == "Exam matrix updated successfully")
            {
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        // Update tag
        [HttpPost("{examMatrixId}/tags/{tagName}")]
        public async Task<IActionResult> UpdateTags(string examMatrixId, [FromBody] ExamMatrixUpdateDto tagsData)
        {
            var result = await this._examMatrixsService.UpdateTag(examMatrixId, tagsData);
            if (result == "Tags updated successfully")
            {
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        [HttpDelete("delete-exam-matrix")]
        public async Task<IActionResult> DeleteMatrixExam(string examMatrixId, string matrixLogUserId)
        {
            var result = await this._examMatrixsService.DeleteExamMatrix(examMatrixId, matrixLogUserId);
            if (result == "Exam matrix marked as unavailable")
            {
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        [HttpDelete("delete-tag")]
        public async Task<IActionResult> DeleteTagExam(string examMatrixId, string tagName, string matrixLogUserId)
        {
            var result = await this._examMatrixsService.DeleteTag(examMatrixId, tagName, matrixLogUserId);
            if (result == "Tag deleted successfully")
            {
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        // Insert form data
        [HttpGet("seed")]
        public async Task<IActionResult> SeedData()
        {
            await this._examMatrixsService.SeedData();
            return new OkObjectResult(new { status = "Success", message = "Example exam matrix data seeded successfully." });
        }
    }
}
