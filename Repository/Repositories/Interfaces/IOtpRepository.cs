using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IOtpRepository
{
    Task<OtpVerification?> GetLatestOtpByEmailAsync(string email);
    Task<OtpVerification?> GetOtpByIdAsync(int otpId);
    Task<bool> IsOtpValidAsync(string email, string otpCode);
    Task AddOtpAsync(OtpVerification otp);
    Task DeleteOtpAsync(int otpId);
    Task SaveAsync();
}
