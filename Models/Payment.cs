using System.ComponentModel.DataAnnotations;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [MaxLength(50)]
    public required string PaymentMethod { get; set; }

    [MaxLength(50)]
    public required string PaymentStatus { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(255)]
    public string? ProviderTransactionId { get; set; }

    [MaxLength(4)]
    public string? CardLastFour { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
