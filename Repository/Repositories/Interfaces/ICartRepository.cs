using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetActiveCartByUserIdAsync(Guid userId);
    Task<Cart> CreateCartAsync(Guid userId);
    Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId);
    Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId);
    Task AddCartItemAsync(CartItem item);
    Task UpdateCartItemAsync(CartItem item);
    Task RemoveCartItemAsync(CartItem item);
    Task RecalculateTotalAsync(Guid cartId);
    Task UpdateCartStatusAsync(Guid cartId, string status);
}
