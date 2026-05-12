using System.ComponentModel.DataAnnotations;

public class Restaurant
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(255)]
    public required string Address { get; set; }

    [MaxLength(25)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = [];

    public ICollection<Order> Orders { get; set; } = [];
}
