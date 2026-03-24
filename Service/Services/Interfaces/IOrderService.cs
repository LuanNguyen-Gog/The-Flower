using Service.DTOs.Orders;

namespace Service.Services.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, string ipAddress);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
    Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task<bool> HandleVnPayReturnAsync(IEnumerable<KeyValuePair<string, string>> queryParams);
}
