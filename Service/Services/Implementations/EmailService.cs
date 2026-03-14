using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Service.Services.Interfaces;
using Service.EmailTemplates;

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
        var template = new OtpVerificationTemplate(otpCode);
        await SendEmailAsync(toEmail, template.GetSubject(), template.GetHtmlBody(), isHtml: true);
    }
}
