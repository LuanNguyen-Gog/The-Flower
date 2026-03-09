using Service.DTOs.Cart;

namespace Service.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemAsync(int userId, AddToCartDto dto);
    Task<CartDto> UpdateItemQuantityAsync(int userId, int cartItemId, UpdateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
    Task<int> GetCartItemCountAsync(int userId);
}
