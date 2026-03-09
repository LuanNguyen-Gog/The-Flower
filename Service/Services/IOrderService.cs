using PayOS.Models.Webhooks;
using Service.DTOs.Orders;

namespace Service.Services;

public interface IOrderService
{
    Task<CreateOrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task HandlePayOsWebhookAsync(Webhook webhookBody);
}
