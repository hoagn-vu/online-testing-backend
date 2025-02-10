using backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend_online_testing.Services
{
    public class TestService
    {
        private readonly IMongoCollection<ProductModel> _productCollection;

        public TestService(IMongoDatabase database)
        {
            _productCollection = database.GetCollection<ProductModel>("Products");
        }

        public async Task<List<ProductModel>> GetProducts()
        {
            return _productCollection.Find(FilterDefinition<ProductModel>.Empty).ToList();
        }

        public async Task<string> AddOrUpdateProduct(ProductModel product)
        {
            var prodObj = _productCollection.Find(x => x.Id == product.Id).FirstOrDefault();
            if (prodObj == null)
            {
                _productCollection.InsertOne(product);
                return "Add new successful";
            } else
            {
                _productCollection.ReplaceOne(x => x.Id == product.Id, product);
            }
            return "Update successful";
        }

        public async Task<string> DeleteProduct(ObjectId productId)
        {
            var prod = _productCollection.DeleteOne(x => x.Id == productId);
            if (prod.DeletedCount > 0)
            {
                return "Deleted";
            }
            return " Product Not Found";
        }
    }
}
