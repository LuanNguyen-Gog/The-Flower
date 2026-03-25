using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Orders;

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
