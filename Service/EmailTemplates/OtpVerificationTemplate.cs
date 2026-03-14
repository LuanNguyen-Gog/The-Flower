namespace Service.EmailTemplates;

public class OtpVerificationTemplate : IEmailTemplate
{
    private readonly string _otpCode;

    public OtpVerificationTemplate(string otpCode)
    {
        _otpCode = otpCode;
    }

    public string GetSubject() => "The Flower - Xác Minh OTP";

    public string GetHtmlBody()
    {
        // Read HTML template
        var templatePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Service", "EmailTemplates", "Html", "OtpVerification.html");
        var htmlContent = File.ReadAllText(templatePath);

        // Replace placeholders
        htmlContent = htmlContent.Replace("{{OTP_CODE}}", _otpCode);

        return htmlContent;
    }
}
