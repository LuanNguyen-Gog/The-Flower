using Repository.Repositories.Interfaces;
using Service.DTOs.Dashboard;
using Service.DTOs.Orders;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var allOrders = await _orderRepository.GetAllWithDetailsAsync();
        var allProducts = await _productRepository.GetAllAsync();
        var allUsers = await _userRepository.GetAllAsync();

        var paidOrders = allOrders.Where(o => o.OrderStatus == "Paid" || o.OrderStatus == "Confirmed" || o.OrderStatus == "Shipping" || o.OrderStatus == "Completed");
        
        var totalSales = paidOrders.Sum(o => o.Payments.FirstOrDefault(p => p.PaymentStatus == "Success" || p.PaymentStatus == "COD")?.Amount ?? 0);

        // Top products calculation
        var topProducts = allOrders
            .Where(o => o.OrderStatus != "Cancelled" && o.OrderStatus != "PaymentFailed")
            .SelectMany(o => o.Cart?.CartItems ?? Enumerable.Empty<Repository.Models.CartItem>())
            .GroupBy(ci => ci.Product?.ProductName ?? "Unknown")
            .Select(g => new TopProductDto
            {
                Name = g.Key,
                QuantitySold = g.Sum(ci => ci.Quantity)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(5)
            .ToList();

        // Monthly revenue calculation (Last 6 months)
        var monthlyRevenue = new List<MonthlyRevenueDto>();
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            var monthName = date.ToString("MMM yyyy");
            
            var monthlySales = paidOrders
                .Where(o => o.OrderDate.Month == date.Month && o.OrderDate.Year == date.Year)
                .Sum(o => o.Payments.FirstOrDefault(p => p.PaymentStatus == "Success" || p.PaymentStatus == "COD")?.Amount ?? 0);

            monthlyRevenue.Add(new MonthlyRevenueDto
            {
                Month = monthName,
                Revenue = (double)monthlySales
            });
        }

        return new DashboardStatsDto
        {
            TotalSales = (double)totalSales,
            TotalOrders = allOrders.Count(),
            TotalUsers = allUsers.Count,
            TotalProducts = allProducts.Count(),
            TopProducts = topProducts,
            RecentOrders = allOrders.Take(5).Select(MapToOrderDto).ToList(),
            MonthlyRevenue = monthlyRevenue
        };
    }

    private static OrderDto MapToOrderDto(Repository.Models.Order order) => new()
    {
        OrderId = order.OrderId,
        PaymentMethod = order.PaymentMethod,
        BillingAddress = order.BillingAddress,
        OrderStatus = order.OrderStatus,
        OrderDate = order.OrderDate,
        TotalAmount = order.Cart?.TotalPrice ?? 0,
        PaymentStatus = order.Payments.FirstOrDefault()?.PaymentStatus ?? "Pending",
        Items = order.Cart?.CartItems.Select(ci => new OrderItemDto
        {
            ProductId = ci.ProductId ?? Guid.Empty,
            ProductName = ci.Product?.ProductName ?? string.Empty,
            ImageUrl = ci.Product?.ImageUrl,
            UnitPrice = ci.Price,
            Quantity = ci.Quantity,
            SubTotal = ci.Price * ci.Quantity
        }).ToList() ?? new List<OrderItemDto>()
    };
}
