#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver;

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly TestService _productService;

        public ProductController(TestService productService)
        {
            this._productService = productService;
        }

        // [HttpGet]
        // public async Task<ActionResult<List<ProductModel>>> Get() =>
        //    Ok(await _productService.GetProducts());

        // [HttpPost]
        // public async Task<IActionResult> Create(ProductModel product)
        // {
        //    await _productService.AddOrUpdateProduct(product);
        //    return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        // }
    }
}
