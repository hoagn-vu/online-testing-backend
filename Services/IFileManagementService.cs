namespace Backend_online_testing.Services
{
    public interface IFileManagementService
    {
        Task<string> ProcessFileTxt(StreamReader reader, string subjectId, string questionBankId);
        Task<string> ProcessFileDocx(Stream stream, string subjectId, string questionBankId);
    }
}
