using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using PayOS.Resources.V2.PaymentRequests;
using PayOS.Resources.Webhooks;
using Repository.Models;
using Repository.Repositories;
using Service.DTOs.Orders;

namespace Service.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly PayOSClient _payOsClient;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;

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
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId)
            ?? throw new InvalidOperationException("No active cart found.");

        if (!cart.CartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        var order = await _orderRepository.CreateAsync(new Order
        {
            CartId = cart.CartId,
            UserId = userId,
            PaymentMethod = dto.PaymentMethod,
            BillingAddress = dto.BillingAddress,
            OrderStatus = "Pending",
            OrderDate = DateTime.UtcNow
        });

        await _orderRepository.CreatePaymentAsync(new Payment
        {
            OrderId = order.OrderId,
            Amount = cart.TotalPrice,
            PaymentDate = DateTime.UtcNow,
            PaymentStatus = "Pending"
        });

        await _cartRepository.UpdateCartStatusAsync(cart.CartId, "CheckedOut");

        string? paymentUrl = null;

        if (dto.PaymentMethod.Equals("PayOS", StringComparison.OrdinalIgnoreCase))
        {
            paymentUrl = await CreatePayOsLinkAsync(order.OrderId, cart, dto);
        }
        else
        {
            order.OrderStatus = "Confirmed";
            await _orderRepository.UpdateOrderAsync(order);

            var payment = await _orderRepository.GetPaymentByOrderIdAsync(order.OrderId);
            if (payment is not null)
            {
                payment.PaymentStatus = "COD";
                await _orderRepository.UpdatePaymentAsync(payment);
            }
        }

        return new CreateOrderResponseDto
        {
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            PaymentMethod = order.PaymentMethod,
            TotalAmount = cart.TotalPrice,
            PaymentUrl = paymentUrl
        };
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
