using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace Service.Services.Implementations;

/// <summary>
/// Helper class để build VnPay payment URL và verify callback signature.
/// VnPay không có SDK chính thức — sử dụng HMAC-SHA512 tự implement.
/// </summary>
public static class VnPayHelper
{
    /// <summary>
    /// Build URL thanh toán VnPay từ thông tin đơn hàng.
    /// </summary>
    public static string BuildPaymentUrl(
        Guid orderId,
        long amountVnd,
        string returnUrl,
        string ipAddress,
        string tmnCode,
        string hashSecret,
        string baseUrl)
    {
        // VnPay yêu cầu amount * 100
        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"]    = "2.1.0",
            ["vnp_Command"]    = "pay",
            ["vnp_TmnCode"]    = tmnCode,
            ["vnp_Amount"]     = (amountVnd * 100).ToString(),
            ["vnp_CurrCode"]   = "VND",
            ["vnp_TxnRef"]     = orderId.ToString(),
            ["vnp_OrderInfo"]  = $"Thanh toan don hang {orderId}",
            ["vnp_OrderType"]  = "other",
            ["vnp_Locale"]     = "vn",
            ["vnp_ReturnUrl"]  = returnUrl,
            ["vnp_IpAddr"]     = ipAddress,
            ["vnp_CreateDate"] = GetVnPayTime(DateTime.UtcNow),
            ["vnp_ExpireDate"] = GetVnPayTime(DateTime.UtcNow.AddMinutes(15)),
        };

        // VNPay yêu cầu:
        // - hashData: key=value chưa URL-encode
        // - query: key=value đã URL-encode
        var hashData = BuildHashData(vnpParams);
        var queryData = BuildQueryData(vnpParams);
        var secureHash = HmacSha512(hashSecret, hashData);

        return $"{baseUrl}?{queryData}&vnp_SecureHash={secureHash}";
    }

    /// <summary>
    /// Xác minh chữ ký từ VnPay callback (Return URL).
    /// </summary>
    public static bool VerifySignature(
        IEnumerable<KeyValuePair<string, string>> queryParams,
        string hashSecret)
    {
        var paramList = queryParams.ToList();

        var receivedHash = paramList
            .FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;

        if (string.IsNullOrEmpty(receivedHash))
            return false;

        // Loại bỏ vnp_SecureHash và vnp_SecureHashType trước khi tính
        var filteredParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in paramList)
        {
            if (p.Key != "vnp_SecureHash" && p.Key != "vnp_SecureHashType")
                filteredParams[p.Key] = p.Value;
        }

        var hashData = BuildHashData(filteredParams);
        var computedHash = HmacSha512(hashSecret, hashData);

        return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra ResponseCode == "00" (thành công).
    /// </summary>
    public static bool IsSuccess(IEnumerable<KeyValuePair<string, string>> queryParams)
        => queryParams.FirstOrDefault(p => p.Key == "vnp_ResponseCode").Value == "00";

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string BuildHashData(SortedDictionary<string, string> sortedParams)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in sortedParams)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(key);
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(value));
            }
        }
        return sb.ToString();
    }

    private static string BuildQueryData(SortedDictionary<string, string> sortedParams)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in sortedParams)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(WebUtility.UrlEncode(key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(value));
            }
        }
        return sb.ToString();
    }

    private static string HmacSha512(string key, string data)
    {
        var keyBytes  = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLower();
    }

    /// <summary>
    /// VnPay yêu cầu thời gian theo timezone GMT+7, format yyyyMMddHHmmss.
    /// </summary>
    private static string GetVnPayTime(DateTime utcTime)
    {
        var vnTime = utcTime.AddHours(7);
        return vnTime.ToString("yyyyMMddHHmmss");
    }
}
