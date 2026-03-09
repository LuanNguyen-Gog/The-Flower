using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly SalesAppDBContext _context;

    public OrderRepository(SalesAppDBContext context) => _context = context;

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        => await _context.Orders
            .Include(o => o.Cart)
            .ThenInclude(c => c!.CartItems)
            .ThenInclude(ci => ci.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        => await _context.Orders
            .Include(o => o.Cart)
            .ThenInclude(c => c!.CartItems)
            .ThenInclude(ci => ci.Product)
            .Include(o => o.Payments)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        => await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}
