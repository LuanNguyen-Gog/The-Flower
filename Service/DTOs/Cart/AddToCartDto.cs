using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Cart;

public class AddToCartDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
    public int Quantity { get; set; }
}
