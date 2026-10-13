using System.ComponentModel.DataAnnotations;

namespace InventoryOrders.Api.Models
{
    public class Order
    {
        public int id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Created";

    }
}
