using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Backend_online_testing.Repositories;
public class ProcessTakeExamRepository
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    private readonly IMongoCollection<RoomsModel> _roomsCollection;
    private readonly IMongoCollection<UsersModel> _usersCollection;
    private readonly IMongoCollection<ExamsModel> _examsCollection;
    private readonly IMongoCollection<ExamMatricesModel> _examMatricesCollection;

    public ProcessTakeExamRepository(IMongoDatabase database)
    {
        _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        _roomsCollection = database.GetCollection<RoomsModel>("rooms");
        _usersCollection = database.GetCollection<UsersModel>("users");
        _examsCollection = database.GetCollection<ExamsModel>("exams");
        _examMatricesCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
    }


    /*
     * Filter definition
     */

    // Filter by organize exam id
    public FilterDefinition<OrganizeExamModel> FilterByOrganizeExamId(string organizeExamId)
    {
        return Builders<OrganizeExamModel>.Filter.Eq(x=>x.Id, organizeExamId);
    }


    /*
     * Function
     */

    // Get session by id
    public async Task<OrganizeExamModel?> GetSessionAsync(string organizeExamId, string sessionId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(x => x.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );

        return await _organizeExamCollection.Find(filter).FirstOrDefaultAsync();
    }
    
    public async Task<bool> UpdateUsersTrackExamsStatusAsync(List<string> userIds, string organizeExamId, string sessionId, string newStatus)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.In(u => u.Id, userIds),
            Builders<UsersModel>.Filter.ElemMatch(u => u.TrackExam, t => 
                t.OrganizeExamId == organizeExamId && t.SessionId == sessionId)
        );

        // var update = Builders<UsersModel>.Update
        //     .Set("trackExams.$.roomSessionStatus", newStatus);
        //
        // var result = await _usersCollection.UpdateManyAsync(filter, update);
        var update = Builders<UsersModel>.Update
            .Set("trackExams.$[elem].roomSessionStatus", newStatus);

        var options = new UpdateOptions
        {
            ArrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("elem.organizeExamId", organizeExamId)
                        .Add("elem.sessionId", sessionId))
            }
        };

        var result = await _usersCollection.UpdateManyAsync(filter, update, options);
 
        return result.ModifiedCount > 0;
    }

    // Update session status
    public async Task<bool> UpdateSessionStatusAsync(string organizeExamId, string sessionId, string newStatus)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            FilterByOrganizeExamId(organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );

        var update = Builders<OrganizeExamModel>.Update
            .Set("sessions.$.sessionStatus", newStatus);

        var result = await _organizeExamCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    //Increate Violation Count
    public async Task<bool> IncreaseViolationCountRepository(string userId, string takeExamId)
    {
        var filter = Builders<UsersModel>.Filter.And(
                Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
                Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam, te => te.Id == takeExamId)
            );

        var update = Builders<UsersModel>.Update.Inc("takeExams.$.violationCount", 1);

        var result = await _usersCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }
    //Update Status And Reason
    public async Task<bool> UpdateStatusAndReasonRepository(string userId, string takeExamId, string type, string? unrecognizedReason)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
            Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam, te => te.Id == takeExamId)
        );

        var updates = new List<UpdateDefinition<UsersModel>>
        {
            Builders<UsersModel>.Update.Set("takeExams.$.status", type)
        };

        if (type == "active")
        {
            updates.Add(Builders<UsersModel>.Update.Set("takeExams.$.answers", new List<AnswersModel>()));
        }

        if (!string.IsNullOrEmpty(unrecognizedReason))
        {
            updates.Add(Builders<UsersModel>.Update.Set("takeExams.$.unrecognizedReason", unrecognizedReason));
        }

        var update = Builders<UsersModel>.Update.Combine(updates);
        var result = await _usersCollection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }


    /*
     * Room
     */
    //Get room by id
    public async Task<RoomsModel> GetRoomByRoomIdAsync(string roomId)
    {
        return await _roomsCollection.Find(r => r.Id == roomId).FirstOrDefaultAsync();
    }

    //Update room status 
    public async Task<bool> UpdateRoomStatusAsync(string organizeExamId, string sessionId, string roomId, string newStatus)
    {
        var filter = Builders<OrganizeExamModel> .Filter.And(
            FilterByOrganizeExamId(organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );

        var update = Builders <OrganizeExamModel>.Update
            .Set("sessions.$.rooms.$[room].roomStatus", newStatus);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new JsonArrayFilterDefinition<BsonDocument>($"{{ 'room.roomId': '{roomId}' }}")
        };

        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
        var result = await _organizeExamCollection.UpdateOneAsync(filter, update, updateOptions);

        return result.ModifiedCount > 0;
    }

    /*
     * User
     */
    //Update candidate rooms status
    public async Task UpdateCandidateRoomStatusAsync(
        IEnumerable<string> candidateIds, 
        // IEnumerable<string> supervisorIds, 
        string newStatus)
    {
        if (candidateIds != null && candidateIds.Any())
        {
            var candidateFilter = Builders<UsersModel>.Filter.In(u => u.Id, candidateIds);
            var candidateUpdate = Builders<UsersModel>.Update
                .Set("takeExams.$[].status", newStatus);

            await _usersCollection.UpdateManyAsync(candidateFilter, candidateUpdate);
        }

        // if (supervisorIds != null && supervisorIds.Any())
        // {
        //     var supervisorFilter = Builders<UsersModel>.Filter.In(u => u.Id, supervisorIds);
        //     var supervisorUpdate = Builders<UsersModel>.Update
        //         .Set("trackExams.$[].status", newStatus);
        //
        //     await _usersCollection.UpdateManyAsync(supervisorFilter, supervisorUpdate);
        // }
    }

    //Get by user id
    public async Task<UsersModel?> GetByUserIdAsync(string userId)
    {
        return await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }
    
    public async Task<UsersModel?> GetByUserIdAndUpdateFinishTimeAsync(string userId, string takeExamId)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
            Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam, te => te.Id == takeExamId)
        );
        
        var update = Builders<UsersModel>.Update.Set("takeExams.$.finishedAt", DateTime.UtcNow);
        var result = await _usersCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0 ? await _usersCollection.Find(filter).FirstOrDefaultAsync() : null;
    }
    
    //Get candidate by multiple ids
    public async Task<List<UsersModel>> GetCandidatesByIdsAsync(List<string> candidateIds)
    {
        return await _usersCollection
            .Find(u => candidateIds.Contains(u.Id))
            .ToListAsync();
    }


    /*
     * Exam
     */
    //Update take exam 
    public async Task UpdateTakeExamsAsync(string userId, List<TakeExamsModel> takeExams)
    {
        var update = Builders<UsersModel>.Update.Set(u => u.TakeExam, takeExams);
        await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);
    }


    /*
     * Organize exam
     */
    //Get organize exam by id
    public async Task<OrganizeExamModel?> GetOrganizeExamByIdAsync(string organizeExamId)
    {
        return await _organizeExamCollection.Find(oe => oe.Id == organizeExamId).FirstOrDefaultAsync();
    }


    /*
     * Subject 
     */
    public async Task<SubjectsModel> GetSubjectByIdAsync(string id)
    {
        return await _subjectsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }
    
    
    
    
    // Lấy kỳ thi
    public async Task<OrganizeExamModel?> GetOrganizeExamAsync(string organizeExamId)
    {
        return await _organizeExamCollection
            .Find(x => x.Id == organizeExamId)
            .FirstOrDefaultAsync();
    }

    public async Task<UsersModel?> GetUserByIdAsync(string userId)
    {
        return await _usersCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
    }

    public async Task UpdateUserTakeExamRoomStatusAsync(
        string userId, string organizeExamId, string sessionId, string roomId, string newStatus)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
            Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam,
                te => te.OrganizeExamId == organizeExamId &&
                      te.SessionId == sessionId &&
                      te.RoomId == roomId)
        );

        var update = Builders<UsersModel>.Update
            .Set("takeExams.$.roomStatus", newStatus);

        await _usersCollection.UpdateOneAsync(filter, update);
    }

}
