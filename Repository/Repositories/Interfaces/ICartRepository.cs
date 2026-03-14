using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetActiveCartByUserIdAsync(int userId);
    Task<Cart> CreateCartAsync(int userId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task AddCartItemAsync(CartItem item);
    Task UpdateCartItemAsync(CartItem item);
    Task RemoveCartItemAsync(CartItem item);
    Task RecalculateTotalAsync(int cartId);
    Task UpdateCartStatusAsync(int cartId, string status);
}
