namespace Service.DTOs.Cart;

public class CartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
}

public class CartDto
{
    public Guid CartId { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
}
