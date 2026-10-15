using InventoryOrders.Api.Controllers;
using InventoryOrders.Api.Data;
using InventoryOrders.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryOrders.Tests;

public class OrdersControllerTests
{
    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateOrder_WithEnoughStock_ReducesStock()
    {
        var db = CreateDatabase();
        var product = new Product
        {
            Name = "Keyboard",
            Sku = "KB-001",
            Price = 49.99m,
            StockQuantity = 10
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var orderController = new OrdersController(db);
        var order = new Order
        {

            ProductId = product.Id,
            Quantity = 3
        };

        var response = await orderController.CreateOrder(order);
        Assert.IsType<CreatedAtActionResult>(response.Result);
        var updatedProduct = await db.Products.FindAsync(product.Id);
        Assert.Equal(7, updatedProduct?.StockQuantity);
        Assert.Single(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task CreateOrder_WithInsufficientStock_RejectsOrder()
    {
        using var db = CreateDatabase();

        var product = new Product
        {
            Name = "Mouse",
            Sku = "MS-001",
            Price = 19.99m,
            StockQuantity = 2
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var controller = new OrdersController(db);

        var order = new Order
        {
            ProductId = product.Id,
            Quantity = 5
        };

        var response = await controller.CreateOrder(order);

        Assert.IsType<ConflictObjectResult>(response.Result);

        var unchangedProduct = await db.Products.FindAsync(product.Id);
        Assert.Equal(2, unchangedProduct?.StockQuantity);

        Assert.Empty(await db.Orders.ToListAsync());
    }

}
