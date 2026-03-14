namespace Service.EmailTemplates;

public class StaffCredentialsTemplate : IEmailTemplate
{
    private readonly string _username;
    private readonly string _password;

    public StaffCredentialsTemplate(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public string GetSubject() => "🌸 The Flower - Thông tin đăng nhập Staff";

    public string GetHtmlBody()
    {
        // Read HTML template
        var templatePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Service", "EmailTemplates", "Html", "StaffCredentials.html");
        var htmlContent = File.ReadAllText(templatePath);

        // Replace placeholders
        htmlContent = htmlContent.Replace("{{USERNAME}}", _username)
            .Replace("{{PASSWORD}}", _password);

        return htmlContent;
    }
}
