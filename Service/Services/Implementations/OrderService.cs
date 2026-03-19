using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using PayOS.Resources.V2.PaymentRequests;
using PayOS.Resources.Webhooks;
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
    private readonly PayOSClient _payOsClient;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;

        var cfg = configuration.GetSection("PayOS");
        _payOsClient = new PayOSClient(new PayOSOptions
        {
            ClientId = cfg["ClientId"]!,
            ApiKey = cfg["ApiKey"]!,
            ChecksumKey = cfg["ChecksumKey"]!
        });
    }

    public async Task<CreateOrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.BillingAddress))
                throw new InvalidOperationException("Billing address is required.");

            if (dto.PaymentMethod.Equals("PayOS", StringComparison.OrdinalIgnoreCase) 
                && (string.IsNullOrWhiteSpace(dto.ReturnUrl) || string.IsNullOrWhiteSpace(dto.CancelUrl)))
                throw new InvalidOperationException("Return URL and Cancel URL are required for PayOS payment.");

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

                // Handle payment method
                string? paymentUrl = null;

                if (dto.PaymentMethod.Equals("PayOS", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        paymentUrl = await CreatePayOsLinkAsync(order.OrderId, cart, dto);
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new InvalidOperationException("Failed to connect to PayOS service. Please try again later.", ex);
                    }
                    catch (PayOSException ex)
                    {
                        throw new InvalidOperationException($"PayOS error: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("An unexpected error occurred while creating PayOS payment link.", ex);
                    }
                }
                else
                {
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
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync();

                return new CreateOrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderStatus = order.OrderStatus,
                    PaymentMethod = order.PaymentMethod,
                    TotalAmount = cart.TotalPrice,
                    PaymentUrl = paymentUrl
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

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order is null || order.UserId != userId) return null;
        return MapToDto(order);
    }

    public async Task HandlePayOsWebhookAsync(Webhook webhookBody)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            WebhookData data;
            try
            {
                var webhooksResource = new Webhooks(_payOsClient);
                data = await webhooksResource.VerifyAsync(webhookBody);
            }
            catch (WebhookException)
            {
                throw new UnauthorizedAccessException("Invalid PayOS webhook signature.");
            }

            var orderId = (int)data.OrderCode;
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order is null) return;

            var isSuccess = data.Code == "00";
            order.OrderStatus = isSuccess ? "Paid" : "PaymentFailed";
            await _orderRepository.UpdateOrderAsync(order);

            var payment = await _orderRepository.GetPaymentByOrderIdAsync(orderId);
            if (payment is not null)
            {
                payment.PaymentStatus = isSuccess ? "Success" : "Failed";
                await _orderRepository.UpdatePaymentAsync(payment);
            }

            // Soft-delete cart and update stock when payment successful
            if (isSuccess && order.CartId.HasValue)
            {
                await _cartRepository.UpdateCartStatusAsync(order.CartId.Value, "Payed");

                // Update stock quantity for each product in cart
                if (order.Cart?.CartItems.Any() == true)
                {
                    foreach (var cartItem in order.Cart.CartItems)
                    {
                        if (cartItem.Product is not null)
                        {
                            cartItem.Product.StockQuantity = (cartItem.Product.StockQuantity ?? 0) - cartItem.Quantity;
                        }
                    }
                }

                // Create success notification
                if (order.UserId.HasValue)
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            order.UserId.Value,
                            $"Đơn hàng #{order.OrderId} thanh toán thành công!");
                    }
                    catch (Exception ex)
                    {
                        // Log notification error but don't throw
                        Console.WriteLine($"Failed to create notification: {ex.Message}");
                    }
                }
            }

            // Save all changes
            await _unitOfWork.SaveChangesAsync();
        });
    }

    private async Task<string> CreatePayOsLinkAsync(int orderId, Cart cart, CreateOrderDto dto)
    {
        var items = cart.CartItems.Select(ci => new PaymentLinkItem
        {
            Name = ci.Product?.ProductName ?? $"Product {ci.ProductId}",
            Quantity = ci.Quantity,
            Price = (int)ci.Price
        }).ToList();

        var request = new CreatePaymentLinkRequest
        {
            OrderCode = (long)orderId,
            Amount = (int)cart.TotalPrice,
            Description = $"DonHang {orderId}",
            Items = items,
            CancelUrl = dto.CancelUrl ?? "https://yourapp.com/payment/cancel",
            ReturnUrl = dto.ReturnUrl ?? "https://yourapp.com/payment/success"
        };

        var paymentRequestsResource = new PaymentRequests(_payOsClient);
        var result = await paymentRequestsResource.CreateAsync(request);
        return result.CheckoutUrl;
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
            ProductId = ci.ProductId ?? 0,
            ProductName = ci.Product?.ProductName ?? string.Empty,
            ImageUrl = ci.Product?.ImageUrl,
            UnitPrice = ci.Price,
            Quantity = ci.Quantity,
            SubTotal = ci.Price * ci.Quantity
        }).ToList() ?? []
    };
}
