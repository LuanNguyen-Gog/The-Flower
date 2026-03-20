using PayOS.Models.Webhooks;
using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Orders;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<CreateOrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.BillingAddress))
                throw new InvalidOperationException("Billing address is required.");

            if (dto.PaymentMethod.Equals("PayOS", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("PayOS tạm thời chưa hỗ trợ khi hệ thống dùng Guid ID.");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
                    ?? throw new InvalidOperationException("No active cart found.");

                if (!cart.CartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                // Validate cart data
                if (cart.TotalPrice <= 0)
                    throw new InvalidOperationException("Cart total price is invalid.");

                if (cart.CartItems.Any(ci => ci.Product is null))
                    throw new InvalidOperationException("Cart contains invalid product(s).");

                // Create order
                var order = await _orderRepository.CreateAsync(new Order
                {
                    CartId = cart.CartId,
                    UserId = userId,
                    PaymentMethod = dto.PaymentMethod,
                    BillingAddress = dto.BillingAddress,
                    OrderStatus = "Pending",
                    OrderDate = DateTime.UtcNow
                });

                // Create payment record
                await _orderRepository.CreatePaymentAsync(new Payment
                {
                    OrderId = order.OrderId,
                    Amount = cart.TotalPrice,
                    PaymentDate = DateTime.UtcNow,
                    PaymentStatus = "Pending"
                });

                // Update cart status to CheckedOut
                await _cartRepository.UpdateCartStatusAsync(cart.CartId, "CheckedOut");

                // COD payment
                order.OrderStatus = "Confirmed";
                await _orderRepository.UpdateOrderAsync(order);

                var payment = await _orderRepository.GetPaymentByOrderIdAsync(order.OrderId);
                if (payment is not null)
                {
                    payment.PaymentStatus = "COD";
                    await _orderRepository.UpdatePaymentAsync(payment);
                }

                try
                {
                    await _notificationService.SendNotificationAsync(
                        userId,
                        $"Đơn hàng #{order.OrderId} đã được xác nhận! Sẽ giao hàng tại địa chỉ: {order.BillingAddress}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send COD notification: {ex.Message}");
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync();

                return new CreateOrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderStatus = order.OrderStatus,
                    PaymentMethod = order.PaymentMethod,
                    TotalAmount = cart.TotalPrice,
                    PaymentUrl = null
                };
            });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while creating the order. Please try again later.", ex);
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order is null || order.UserId != userId) return null;
        return MapToDto(order);
    }

    public async Task HandlePayOsWebhookAsync(Webhook webhookBody)
    {
        throw new InvalidOperationException("PayOS tạm thời chưa hỗ trợ khi hệ thống dùng Guid ID.");
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        OrderId = order.OrderId,
        PaymentMethod = order.PaymentMethod,
        BillingAddress = order.BillingAddress,
        OrderStatus = order.OrderStatus,
        OrderDate = order.OrderDate,
        TotalAmount = order.Cart?.TotalPrice ?? 0,
        PaymentStatus = order.Payments.FirstOrDefault()?.PaymentStatus ?? "Pending",
        Items = order.Cart?.CartItems.Select(ci => new OrderItemDto
        {
            ProductId = ci.ProductId ?? Guid.Empty,
            ProductName = ci.Product?.ProductName ?? string.Empty,
            ImageUrl = ci.Product?.ImageUrl,
            UnitPrice = ci.Price,
            Quantity = ci.Quantity,
            SubTotal = ci.Price * ci.Quantity
        }).ToList() ?? []
    };
}
