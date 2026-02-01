namespace HRCE.Components.ProductCard;

/// <summary>
/// Presenter - مسؤول عن تحويل البيانات للعرض
/// </summary>
public class ProductCardPresenter
{
    public ProductCardModel MapBasic(string title, decimal price, bool inStock)
    {
        return new ProductCardModel(
            Title: title,
            Description: "وصف تجريبي للمنتج - يمكن تخصيصه حسب الحاجة",
            ImageUrl: "data:image/svg+xml,%3Csvg width='300' height='200' xmlns='http://www.w3.org/2000/svg'%3E%3Crect width='300' height='200' fill='%23f0f0f0'/%3E%3Ctext x='50%25' y='50%25' font-family='Arial' font-size='20' fill='%23999' text-anchor='middle' dominant-baseline='middle'%3E%D8%B5%D9%88%D8%B1%D8%A9 %D8%A7%D9%84%D9%85%D9%86%D8%AA%D8%AC%3C/text%3E%3C/svg%3E",
            PriceDisplay: $"{price:C}",
            IsInStock: inStock
        );
    }

    // يمكنك إضافة عدة طرق Mapping حسب الحاجة
    // public ProductCardModel MapFromEntity(Product product) 
    // {
    //     return new ProductCardModel(
    //         Title: product.Name,
    //         Description: product.Description,
    //         ImageUrl: product.ImageUrl,
    //         PriceDisplay: $"{product.Price:C}",
    //         IsInStock: product.Stock > 0,
    //         Id: product.Id
    //     );
    // }
}
