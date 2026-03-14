namespace Service.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task SendOtpEmailAsync(string toEmail, string otpCode);
}
