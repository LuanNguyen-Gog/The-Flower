using Repository.Models;

namespace Repository.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<Order?> GetByIdWithDetailsAsync(int orderId);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    Task UpdateOrderAsync(Order order);
    Task UpdatePaymentAsync(Payment payment);
}
