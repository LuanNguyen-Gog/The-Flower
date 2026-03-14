using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class CartRepository : ICartRepository
{
    private readonly SalesAppDBContext _context;

    public CartRepository(SalesAppDBContext context) => _context = context;

    public async Task<Cart?> GetActiveCartByUserIdAsync(int userId)
        => await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart> CreateCartAsync(int userId)
    {
        var cart = new Cart { UserId = userId, TotalPrice = 0};
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        => await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        => await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

    public async Task AddCartItemAsync(CartItem item)
    {
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCartItemAsync(CartItem item)
    {
        _context.CartItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(CartItem item)
    {
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task RecalculateTotalAsync(int cartId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CartId == cartId);

        if (cart is null) return;

        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCartStatusAsync(int cartId, string status)
    {
        var cart = await _context.Carts.FindAsync(cartId);
        if (cart is null) return;
        cart.Status = status;
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
    }
}
