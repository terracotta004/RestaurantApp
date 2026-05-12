using System.ComponentModel.DataAnnotations;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int RestaurantId { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public int? DeliveryAddressId { get; set; }

    public UserAddress? DeliveryAddress { get; set; }

    public int? DeliveryTypeId { get; set; }

    public DeliveryType? DeliveryType { get; set; }

    [MaxLength(50)]
    public required string FulfillmentType { get; set; }

    [MaxLength(50)]
    public required string Status { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    public DateTime OrderedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public ICollection<Payment> Payments { get; set; } = [];

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
