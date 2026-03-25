using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

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
        _configuration = configuration;
    }

    // ── Create Order ─────────────────────────────────────────────────────────

    public async Task<CreateOrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, string ipAddress, string baseUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.BillingAddress))
                throw new InvalidOperationException("Billing address is required.");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
                    ?? throw new InvalidOperationException("No active cart found.");

                if (!cart.CartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                if (cart.TotalPrice <= 0)
                    throw new InvalidOperationException("Cart total price is invalid.");

                if (cart.CartItems.Any(ci => ci.Product is null))
                    throw new InvalidOperationException("Cart contains invalid product(s).");

                // Tạo Order
                var order = await _orderRepository.CreateAsync(new Order
                {
                    CartId        = cart.CartId,
                    UserId        = userId,
                    PaymentMethod = dto.PaymentMethod,
                    BillingAddress = dto.BillingAddress,
                    OrderStatus   = "Pending",
                    OrderDate     = DateTime.UtcNow.AddHours(7)
                });

                // Tạo Payment record
                await _orderRepository.CreatePaymentAsync(new Payment
                {
                    OrderId       = order.OrderId,
                    Amount        = cart.TotalPrice,
                    PaymentDate   = DateTime.UtcNow.AddHours(7),
                    PaymentStatus = "Pending"
                });

                // Cart → CheckedOut
                await _cartRepository.UpdateCartStatusAsync(cart.CartId, "CheckedOut");

                string? paymentUrl = null;

                if (dto.PaymentMethod.Equals("VnPay", StringComparison.OrdinalIgnoreCase))
                {
                    // ── VnPay ─────────────────────────────────────────────
                    var cfg = _configuration.GetSection("VnPay");
                    // Combine baseUrl with the relative ReturnUrl from config
                    var relativeReturnUrl = cfg["ReturnUrl"]!;
                    var fullReturnUrl = relativeReturnUrl.StartsWith("http") 
                        ? relativeReturnUrl 
                        : $"{baseUrl.TrimEnd('/')}/{relativeReturnUrl.TrimStart('/')}";

                    paymentUrl = VnPayHelper.BuildPaymentUrl(
                        orderId:    order.OrderId,
                        amountVnd:  (long)cart.TotalPrice,
                        returnUrl:  fullReturnUrl,
                        ipAddress:  ipAddress,
                        tmnCode:    cfg["TmnCode"]!,
                        hashSecret: cfg["HashSecret"]!,
                        baseUrl:    cfg["BaseUrl"]!);
                }
                else if (dto.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    // ── COD ───────────────────────────────────────────────
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
                else
                {
                    throw new InvalidOperationException($"Payment method '{dto.PaymentMethod}' is not supported. Use 'VnPay' or 'COD'.");
                }

                await _unitOfWork.SaveChangesAsync();

                return new CreateOrderResponseDto
                {
                    OrderId       = order.OrderId,
                    OrderStatus   = order.OrderStatus,
                    PaymentMethod = order.PaymentMethod,
                    TotalAmount   = cart.TotalPrice,
                    PaymentUrl    = paymentUrl
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

    // ── VnPay Return URL Handler ──────────────────────────────────────────────

    public async Task<bool> HandleVnPayReturnAsync(IEnumerable<KeyValuePair<string, string>> queryParams)
    {
        var paramList = queryParams.ToList();

        // Xác minh chữ ký HMAC-SHA512
        var hashSecret = _configuration["VnPay:HashSecret"]!;
        if (!VnPayHelper.VerifySignature(paramList, hashSecret))
            throw new UnauthorizedAccessException("Invalid VnPay signature.");

        // Lấy orderId từ vnp_TxnRef
        var txnRef = paramList.FirstOrDefault(p => p.Key == "vnp_TxnRef").Value;
        if (!Guid.TryParse(txnRef, out var orderId))
            throw new InvalidOperationException("Invalid vnp_TxnRef.");

        var isSuccess = VnPayHelper.IsSuccess(paramList);

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
                ?? throw new InvalidOperationException($"Order {orderId} not found.");

            // Cập nhật trạng thái đơn hàng
            order.OrderStatus = isSuccess ? "Paid" : "PaymentFailed";
            await _orderRepository.UpdateOrderAsync(order);

            // Cập nhật trạng thái thanh toán
            var payment = await _orderRepository.GetPaymentByOrderIdAsync(orderId);
            if (payment is not null)
            {
                payment.PaymentStatus = isSuccess ? "Success" : "Failed";
                await _orderRepository.UpdatePaymentAsync(payment);
            }

            if (isSuccess && order.CartId.HasValue)
            {
                // Cart → Payed
                await _cartRepository.UpdateCartStatusAsync(order.CartId.Value, "Payed");

                // Trừ stock từng sản phẩm
                if (order.Cart?.CartItems.Any() == true)
                {
                    foreach (var cartItem in order.Cart.CartItems)
                    {
                        if (cartItem.Product is not null)
                            cartItem.Product.StockQuantity = (cartItem.Product.StockQuantity ?? 0) - cartItem.Quantity;
                    }
                }

                // Gửi notification
                if (order.UserId.HasValue)
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            order.UserId.Value,
                            $"Đơn hàng #{order.OrderId} thanh toán thành công qua VnPay!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create notification: {ex.Message}");
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return isSuccess;
        });
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailsAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order is null || order.UserId != userId) return null;
        return MapToDto(order);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        order.OrderStatus = dto.Status;
        await _orderRepository.UpdateOrderAsync(order);
        
        // Notify user if possible
        if (order.UserId.HasValue)
        {
            try
            {
                await _notificationService.SendNotificationAsync(order.UserId.Value, 
                    $"Đơn hàng #{order.OrderId.ToString().ToUpper().Substring(0, 8)} của bạn đã được cập nhật trạng thái: {dto.Status}");
            }
            catch { /* Ignore notification failures */ }
        }
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        OrderId       = order.OrderId,
        PaymentMethod = order.PaymentMethod,
        BillingAddress = order.BillingAddress,
        OrderStatus   = order.OrderStatus,
        OrderDate     = order.OrderDate,
        TotalAmount   = order.Cart?.TotalPrice ?? 0,
        PaymentStatus = order.Payments.FirstOrDefault()?.PaymentStatus ?? "Pending",
        Items = order.Cart?.CartItems.Select(ci => new OrderItemDto
        {
            ProductId   = ci.ProductId ?? Guid.Empty,
            ProductName = ci.Product?.ProductName ?? string.Empty,
            ImageUrl    = ci.Product?.ImageUrl,
            UnitPrice   = ci.Price,
            Quantity    = ci.Quantity,
            SubTotal    = ci.Price * ci.Quantity
        }).ToList() ?? []
    };
}
