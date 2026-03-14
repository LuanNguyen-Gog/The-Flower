using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class StoreLocationRepository : IStoreLocationRepository
{
    private readonly SalesAppDBContext _context;

    public StoreLocationRepository(SalesAppDBContext context) => _context = context;

    public async Task<IEnumerable<StoreLocation>> GetAllAsync()
        => await _context.StoreLocations.OrderBy(s => s.LocationId).ToListAsync();

    public async Task<StoreLocation?> GetByIdAsync(int id)
        => await _context.StoreLocations.FindAsync(id);
}
