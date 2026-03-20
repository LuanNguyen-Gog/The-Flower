using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Orders;

public class CreateOrderDto
{
    [Required(ErrorMessage = "Payment method is required.")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required(ErrorMessage = "Billing address is required.")]
    [MaxLength(255)]
    public string BillingAddress { get; set; } = string.Empty;
}
