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

    public async Task<StoreLocation?> GetByIdAsync(Guid id)
        => await _context.StoreLocations.FindAsync(id);

    public async Task<StoreLocation> CreateAsync(StoreLocation location)
    {
        _context.StoreLocations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task<bool> UpdateAsync(StoreLocation location)
    {
        _context.StoreLocations.Update(location);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var location = await _context.StoreLocations.FindAsync(id);
        if (location == null)
            return false;

        location.Status = "InActive";
        _context.StoreLocations.Update(location);
        return await _context.SaveChangesAsync() > 0;
    }
}

