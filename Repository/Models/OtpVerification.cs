using System;

namespace Repository.Models;

public class OtpVerification
{
    public int OtpId { get; set; }

    public int UserId { get; set; }

    public string? Email { get; set; }

    public string? OtpCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; }

    public virtual User? User { get; set; }
}
