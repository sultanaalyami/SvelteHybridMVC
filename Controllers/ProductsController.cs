using Microsoft.AspNetCore.Mvc;
using HRCE.Models;

namespace HRCE.Controllers;

public class ProductsController : Controller
{
    private static readonly List<Product> _products =
    [
        new() { Id = 1, Name = "منتج 1", Price = 99.99m, Stock = 50, Category = "إلكترونيات" },
        new() { Id = 2, Name = "منتج 2", Price = 149.99m, Stock = 30, Category = "إلكترونيات" },
        new() { Id = 3, Name = "منتج 3", Price = 29.99m, Stock = 5, Category = "ملابس" },
    ];

    public IActionResult Index()
    {
        return View(_products.AsEnumerable());
    }

    public IActionResult Edit(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Product product)
    {
        if (!ModelState.IsValid) return View(product);
        
        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
        }
        
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null) _products.Remove(product);
        return RedirectToAction(nameof(Index));
    }
}
