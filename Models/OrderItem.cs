using System.ComponentModel.DataAnnotations;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int MenuItemId { get; set; }

    public MenuItem MenuItem { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [MaxLength(500)]
    public string? SpecialInstructions { get; set; }
}
