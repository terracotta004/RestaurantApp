using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [MaxLength(255)]
    public required string Email { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(25)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<UserAddress> Addresses { get; set; } = [];

    public ICollection<Order> Orders { get; set; } = [];
}
