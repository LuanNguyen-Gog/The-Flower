using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class OtpRepository : IOtpRepository
{
    private readonly SalesAppDBContext _context;

    public OtpRepository(SalesAppDBContext context)
    {
        _context = context;
    }

    public async Task<OtpVerification?> GetLatestOtpByEmailAsync(string email)
    {
        return await _context.OtpVerifications
            .Where(o => o.Email == email && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<OtpVerification?> GetOtpByIdAsync(int otpId)
    {
        return await _context.OtpVerifications.FindAsync(otpId);
    }

    public async Task<bool> IsOtpValidAsync(string email, string otpCode)
    {
        var otp = await GetLatestOtpByEmailAsync(email);
        if (otp == null)
            return false;

        if (otp.IsUsed || DateTime.UtcNow > otp.ExpiresAt)
            return false;

        return otp.OtpCode == otpCode;
    }

    public async Task AddOtpAsync(OtpVerification otp)
    {
        await _context.OtpVerifications.AddAsync(otp);
    }

    public async Task DeleteOtpAsync(int otpId)
    {
        var otp = await GetOtpByIdAsync(otpId);
        if (otp != null)
        {
            _context.OtpVerifications.Remove(otp);
        }
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
