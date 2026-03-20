using Service.DTOs.Cart;

namespace Service.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId);
    Task<CartDto> AddItemAsync(Guid userId, AddToCartDto dto);
    Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId);
    Task ClearCartAsync(Guid userId);
    Task<int> GetCartItemCountAsync(Guid userId);
}
