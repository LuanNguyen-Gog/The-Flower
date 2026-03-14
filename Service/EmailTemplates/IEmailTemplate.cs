namespace Service.EmailTemplates;

public interface IEmailTemplate
{
    string GetSubject();
    string GetHtmlBody();
}
