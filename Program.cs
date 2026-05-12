using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

if (args.Contains("--seed"))
{
    var seedFilePath = Path.Combine(builder.Environment.ContentRootPath, "SeedData.sql");
    await SeedDataRunner.RunAsync(connectionString, seedFilePath);
    return;
}

builder.Services.AddDbContext<Db>((sp, options) =>
{
    options.UseNpgsql(connectionString);
});

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapRestaurantApiEndpoints();

app.Run();

public class Db(DbContextOptions<Db> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<UserAddress> UserAddresses { get; set; } = null!;

    public DbSet<Restaurant> Restaurants { get; set; } = null!;

    public DbSet<MenuItem> MenuItems { get; set; } = null!;

    public DbSet<DeliveryType> DeliveryTypes { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;

    public DbSet<Payment> Payments { get; set; } = null!;

    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<UserAddress>()
            .HasIndex(address => address.UserId);

        modelBuilder.Entity<Restaurant>()
            .HasIndex(restaurant => restaurant.Name);

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(menuItem => menuItem.RestaurantId);
            entity.HasIndex(menuItem => new { menuItem.RestaurantId, menuItem.Name });
            entity.Property(menuItem => menuItem.Price).HasPrecision(10, 2);
            entity.ToTable(table => table.HasCheckConstraint("CK_MenuItems_Price_NonNegative", "\"Price\" >= 0"));
        });

        modelBuilder.Entity<DeliveryType>()
            .HasIndex(deliveryType => deliveryType.Name)
            .IsUnique();

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(order => order.UserId);
            entity.HasIndex(order => order.RestaurantId);
            entity.HasIndex(order => new { order.UserId, order.OrderedAt });
            entity.HasIndex(order => new { order.RestaurantId, order.OrderedAt });
            entity.HasIndex(order => order.Status);
            entity.HasIndex(order => order.FulfillmentType);
            entity.HasIndex(order => order.DeliveryAddressId);
            entity.HasIndex(order => order.DeliveryTypeId);
            entity.Property(order => order.TotalAmount).HasPrecision(10, 2);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Orders_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
                table.HasCheckConstraint("CK_Orders_FulfillmentType", "\"FulfillmentType\" IN ('delivery', 'pickup')");
                table.HasCheckConstraint(
                    "CK_Orders_DeliveryFields_MatchFulfillment",
                    "((\"FulfillmentType\" = 'delivery' AND \"DeliveryAddressId\" IS NOT NULL AND \"DeliveryTypeId\" IS NOT NULL) OR (\"FulfillmentType\" = 'pickup' AND \"DeliveryAddressId\" IS NULL AND \"DeliveryTypeId\" IS NULL))");
            });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(payment => payment.OrderId);
            entity.HasIndex(payment => payment.ProviderTransactionId)
                .IsUnique();
            entity.Property(payment => payment.Amount).HasPrecision(10, 2);
            entity.ToTable(table => table.HasCheckConstraint("CK_Payments_Amount_NonNegative", "\"Amount\" >= 0"));
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(orderItem => orderItem.OrderId);
            entity.HasIndex(orderItem => orderItem.MenuItemId);
            entity.Property(orderItem => orderItem.UnitPrice).HasPrecision(10, 2);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
                table.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
            });
        });
    }
}
