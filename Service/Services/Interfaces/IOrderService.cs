using Service.DTOs.Orders;

namespace Service.Services.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, string ipAddress, string baseUrl);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto);
    Task<bool> HandleVnPayReturnAsync(IEnumerable<KeyValuePair<string, string>> queryParams);
}
