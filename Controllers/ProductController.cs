using backend_online_testing.Models;
using backend_online_testing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_online_testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly TestService _productService;

        public ProductController(TestService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductModel>>> Get() =>
            Ok(await _productService.GetProducts());

        [HttpPost]
        public async Task<IActionResult> Create(ProductModel product)
        {
            await _productService.AddOrUpdateProduct(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
    }
}
