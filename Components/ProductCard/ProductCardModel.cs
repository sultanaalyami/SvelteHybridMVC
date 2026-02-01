namespace HRCE.Components.ProductCard;

/// <summary>
/// Presentation Model لعرض بطاقة المنتج
/// </summary>
public record ProductCardModel(
    string Title,
    string Description,
    string ImageUrl,
    string PriceDisplay,
    bool IsInStock,
    int Id = 0);
