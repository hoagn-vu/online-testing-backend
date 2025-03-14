#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using MongoDB.Driver;

    public class MongoDbService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            this._configuration = configuration;

            var connectionString = this._configuration.GetConnectionString(" ");
            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);

            this._database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoDatabase? Database => this._database;
    }
}
