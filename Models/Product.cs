using System.ComponentModel.DataAnnotations;

namespace SvelteHybridMVC.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "يجب أن يكون اسم المنتج بين 2 و 200 حرف")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "السعر مطلوب")]
    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "المخزون مطلوب")]
    [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون المخزون صفر أو أكثر")]
    public int Stock { get; set; }
    
    [StringLength(1000, ErrorMessage = "يجب ألا يتجاوز الوصف 1000 حرف")]
    public string? Description { get; set; }
    
    [StringLength(100, ErrorMessage = "يجب ألا يتجاوز اسم الفئة 100 حرف")]
    public string? Category { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public ProductFilters Filters { get; set; } = new();
    public IEnumerable<Category> Categories { get; set; } = [];
}

public class ProductFilters
{
    public string? SearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<int> CategoryIds { get; set; } = [];
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
