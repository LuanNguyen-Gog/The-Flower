using Service.DTOs.Cart;

namespace Service.DTOs.Orders;

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
}

public class OrderDto
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = [];
}

public class CreateOrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? PaymentUrl { get; set; }
}
