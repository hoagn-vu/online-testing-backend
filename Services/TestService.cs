#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class TestService
    {
        private readonly IMongoCollection<ProductModel> _productCollection;

        public TestService(IMongoDatabase database)
        {
            this._productCollection = database.GetCollection<ProductModel>("Products");
        }

        // public async Task<List<ProductModel>> GetProducts()
        // {
        //    return this._productCollection.Find(FilterDefinition<ProductModel>.Empty).ToList();
        // }

        // public async Task<string> AddOrUpdateProduct(ProductModel product)
        // {
        //    var prodObj = _productCollection.Find(x => x.Id == product.Id).FirstOrDefault();
        //    if (prodObj == null)
        //    {
        //        _productCollection.InsertOne(product);
        //        return "Add new successful";
        //    }
        //    else
        //    {
        //        _productCollection.ReplaceOne(x => x.Id == product.Id, product);
        //    }
        //    return "Update successful";
        // }

        // public async Task<string> DeleteProduct(ObjectId productId)
        // {
        //    var prod = _productCollection.DeleteOne(x => x.Id == productId);
        //    if (prod.DeletedCount > 0)
        //    {
        //        return "Deleted";
        //    }
        //    return " Product Not Found";
        // }
    }
}
