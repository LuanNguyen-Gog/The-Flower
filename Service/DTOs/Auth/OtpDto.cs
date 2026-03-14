namespace Service.DTOs.Auth;

public class SendOtpRequest
{
    public string? Email { get; set; }
}

public class VerifyOtpRequest
{
    public string? Email { get; set; }
    public string? OtpCode { get; set; }
}

public class OtpResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? ExpiresInMinutes { get; set; }
}

public class VerifyOtpRegisterDto
{
    public string? Email { get; set; }
    public string? OtpCode { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

