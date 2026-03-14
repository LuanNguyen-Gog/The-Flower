using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string? _senderEmail;
    private readonly string? _senderPassword;
    private readonly string? _smtpServer;
    private readonly int _smtpPort;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _senderEmail = _configuration["EmailSettings:SenderEmail"];
        _senderPassword = _configuration["EmailSettings:AppPassword"];
        _smtpServer = _configuration["EmailSettings:SmtpServer"];
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

        if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
        {
            throw new InvalidOperationException("Email configuration is missing in appsettings.json");
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                var mailMessage = new MailMessage(_senderEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                await client.SendMailAsync(mailMessage);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        var subject = "The Flower - Xác Minh OTP";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; color: #333; margin-bottom: 30px; }}
                    .header h1 {{ margin: 0; color: #e91e63; }}
                    .content {{ text-align: center; }}
                    .otp-code {{ font-size: 32px; font-weight: bold; color: #e91e63; letter-spacing: 5px; margin: 30px 0; font-family: monospace; }}
                    .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
                    .warning {{ color: #ff6b6b; margin-top: 20px; font-size: 14px; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>🌸 The Flower</h1>
                    </div>
                    <div class=""content"">
                        <p>Xin chào,</p>
                        <p>Bạn đã yêu cầu xác minh địa chỉ email. Vui lòng sử dụng mã OTP bên dưới để hoàn thành quá trình đăng ký:</p>
                        <div class=""otp-code"">{otpCode}</div>
                        <p>Mã OTP này sẽ hết hạn trong <strong>10 phút</strong>.</p>
                        <div class=""warning"">
                            ⚠️ Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.
                        </div>
                    </div>
                    <div class=""footer"">
                        <p>&copy; 2026 The Flower. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }
}
