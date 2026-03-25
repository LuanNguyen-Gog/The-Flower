using Service.DTOs.Dashboard;

namespace Service.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
