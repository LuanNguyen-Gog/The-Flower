using Repository.Models;

namespace Service.EmailTemplates;

public class OrderPaymentSuccessTemplate : IEmailTemplate
{
    private readonly Order _order;

    public OrderPaymentSuccessTemplate(Order order)
    {
        _order = order;
    }

    public string GetSubject() => "Đơn hàng thành công - The Flower";

    public string GetHtmlBody()
    {
        // Build items table rows
        var itemsHtml = _order.Cart?.CartItems.Select(ci => $@"
                    <tr>
                        <td>{ci.Product?.ProductName ?? "Unknown Product"}</td>
                        <td style='text-align: center;'>{ci.Quantity}</td>
                        <td style='text-align: right;'>{ci.Price:N0} ₫</td>
                        <td style='text-align: right;'>{(ci.Price * ci.Quantity):N0} ₫</td>
                    </tr>
        ").Aggregate("", (acc, html) => acc + html) ?? "";

        // Read HTML template
        var templatePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Service", "EmailTemplates", "Html", "OrderPaymentSuccess.html");
        var htmlContent = File.ReadAllText(templatePath);

        // Replace placeholders
        htmlContent = htmlContent.Replace("{{ORDER_ID}}", $"#{_order.OrderId}")
            .Replace("{{ORDER_DATE}}", _order.OrderDate.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{BILLING_ADDRESS}}", _order.BillingAddress ?? "N/A")
            .Replace("{{PAYMENT_METHOD}}", "PayOS (Thanh toán trực tuyến)")
            .Replace("{{ITEMS_HTML}}", itemsHtml)
            .Replace("{{TOTAL_AMOUNT}}", $"{(_order.Cart?.TotalPrice ?? 0):N0}");

        return htmlContent;
    }
}
