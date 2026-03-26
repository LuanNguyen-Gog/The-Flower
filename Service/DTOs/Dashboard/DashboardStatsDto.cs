using Service.DTOs.Orders;

namespace Service.DTOs.Dashboard;

public class DashboardStatsDto
{
    public double TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<OrderDto> RecentOrders { get; set; } = new();
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public double Revenue { get; set; }
}

public class TopProductDto
{
    public string Name { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}
