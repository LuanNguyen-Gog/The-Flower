using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<Order?> GetByIdWithDetailsAsync(Guid orderId);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId);
    Task UpdateOrderAsync(Order order);
    Task UpdatePaymentAsync(Payment payment);
}
