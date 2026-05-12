using System.ComponentModel.DataAnnotations;

public class MenuItem
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
