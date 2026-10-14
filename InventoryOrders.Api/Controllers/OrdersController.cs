using InventoryOrders.Api.Data;
using InventoryOrders.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryOrders.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        public readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]

        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            if (order.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message = "Order quantity must be greater than zero."
                });

            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == order.ProductId);
            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });

            }
            if (product.StockQuantity < order.Quantity)
            {
                return Conflict(new
                {
                    message = "Insufficient stock.",
                    availableStock = product.StockQuantity
                });

            }

            product.StockQuantity -= order.Quantity;
            order.id = 0;
            order.Product = null;
            order.CreatedAt = DateTime.UtcNow;
            order.Status = "Created";
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
            nameof(GetOrder),
            new { order.id },
            order);

        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var orders = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(p => p.id == id);

            if (orders == null)
            {
                return NotFound(new { message = "Order not found." });

            }
            return Ok(orders);

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetORders()
        {
            var orders = await _context.Orders.AsNoTracking().ToListAsync();
            return Ok(orders);
        }


    }
}
