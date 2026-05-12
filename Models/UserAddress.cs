using System.ComponentModel.DataAnnotations;

public class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [MaxLength(50)]
    public string? Label { get; set; }

    [MaxLength(255)]
    public required string StreetAddress { get; set; }

    [MaxLength(100)]
    public string? ApartmentSuite { get; set; }

    [MaxLength(100)]
    public required string City { get; set; }

    [MaxLength(50)]
    public required string State { get; set; }

    [MaxLength(20)]
    public required string PostalCode { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
