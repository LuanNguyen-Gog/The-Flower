using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Cart;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
            ?? await _cartRepository.CreateCartAsync(userId);
        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid userId, AddToCartDto dto)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId)
                ?? throw new KeyNotFoundException("Product not found.");

            if ((product.StockQuantity ?? 0) < dto.Quantity)
                throw new InvalidOperationException("Insufficient stock.");

            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
                ?? await _cartRepository.CreateCartAsync(userId);

            var existingItem = await _cartRepository.GetCartItemAsync(cart.CartId, dto.ProductId);

            if (existingItem is not null)
            {
                existingItem.Quantity += dto.Quantity;
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                await _cartRepository.AddCartItemAsync(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Price = product.Price
                });
            }

            await _cartRepository.RecalculateTotalAsync(cart.CartId);
            await _unitOfWork.SaveChangesAsync();
            
            return MapToDto((await _cartRepository.GetActiveCartByUserIdAsync(userId))!);
        });
    }

    public async Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemDto dto)
    {
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Active cart not found.");

        var item = await _cartRepository.GetCartItemByIdAsync(cartItemId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        if (item.CartId != cart.CartId)
            throw new UnauthorizedAccessException("Access denied to this cart item.");

        item.Quantity = dto.Quantity;
        await _cartRepository.UpdateCartItemAsync(item);
        await _cartRepository.RecalculateTotalAsync(cart.CartId);

        return MapToDto((await _cartRepository.GetActiveCartByUserIdAsync(userId))!);
    }

    public async Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Active cart not found.");

        var item = await _cartRepository.GetCartItemByIdAsync(cartItemId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        if (item.CartId != cart.CartId)
            throw new UnauthorizedAccessException("Access denied to this cart item.");

        await _cartRepository.RemoveCartItemAsync(item);
        await _cartRepository.RecalculateTotalAsync(cart.CartId);

        return MapToDto((await _cartRepository.GetActiveCartByUserIdAsync(userId))!);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
        if (cart is null) return;

        foreach (var item in cart.CartItems.ToList())
            await _cartRepository.RemoveCartItemAsync(item);

        await _cartRepository.RecalculateTotalAsync(cart.CartId);
    }

    public async Task<int> GetCartItemCountAsync(Guid userId)
    {
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
        return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
    }

    private static CartDto MapToDto(Cart cart) => new()
    {
        CartId = cart.CartId,
        TotalPrice = cart.TotalPrice,
        TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
        Items = cart.CartItems.Select(ci => new CartItemDto
        {
            CartItemId = ci.CartItemId,
            ProductId = ci.ProductId ?? Guid.Empty,
            ProductName = ci.Product?.ProductName ?? string.Empty,
            ImageUrl = ci.Product?.ImageUrl,
            UnitPrice = ci.Price,
            Quantity = ci.Quantity,
            SubTotal = ci.Price * ci.Quantity
        }).ToList()
    };
}
