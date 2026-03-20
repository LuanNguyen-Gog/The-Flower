using PayOS.Models.Webhooks;
using Service.DTOs.Orders;

namespace Service.Services.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
    Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task HandlePayOsWebhookAsync(Webhook webhookBody);
}
