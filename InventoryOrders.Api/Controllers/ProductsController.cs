using InventoryOrders.Api.Data;
using InventoryOrders.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryOrders.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/products

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Product {id} was not found." });
            }
            return Ok(product);
        }

        [Authorize]
        [HttpPost]

        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            product.Id = 0;
            var skuExists = await _context.Products.AnyAsync(p => p.Sku == product.Sku);
            if (skuExists)
            {
                return Conflict(new { message = "A product with this SKU already exists." });

            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);

        }

        [Authorize]
        [HttpPut("{id:int}")]

        public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound(new { message = $"Product {id} was not found." });

            }

            var skuExists =   await _context.Products.AnyAsync(p=>p.Sku == updatedProduct.Sku && p.Id !=id);
            if (skuExists)
                return Conflict(new { message = "A product with this SKU already exists." });
            product.Name = updatedProduct.Name;
            product.Sku = updatedProduct.Sku;
            product.StockQuantity   = updatedProduct.StockQuantity;
            product.Price=  updatedProduct.Price;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                return NoContent();

            }
            return NotFound(new { message = $"Product {id} was not found." });

        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProducts([FromQuery] int minimumStock = 10)
        {
            if(minimumStock < 0)
            {
                return BadRequest(new
                {
                    message = "Minimum stock cannot be negative."
                });
            }

            var products = await _context.Products.FromSqlInterpolated($"Exec dbo.GetLowStockProducts @MinimumStock={minimumStock}")
                .AsNoTracking() .ToListAsync();
            return Ok(products);
        }

    }
}



