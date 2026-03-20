using Service.DTOs.Orders;

namespace Service.Services.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto, string ipAddress);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<bool> HandleVnPayReturnAsync(IEnumerable<KeyValuePair<string, string>> queryParams);
}
