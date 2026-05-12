using System.ComponentModel.DataAnnotations;

public class DeliveryType
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
