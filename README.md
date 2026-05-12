# RestaurantApp
A sample restaurant app

## Seed Data

The app includes sample seed data in `SeedData.sql`. Before running it, make sure the PostgreSQL database from `appsettings.Local.json` is running and migrations have been applied.

Apply migrations:

```powershell
dotnet ef database update
```

Run the seed script:

```powershell
dotnet run -- --seed
```

The seed command is repeatable. It inserts sample users, addresses, delivery types, restaurants, menu items, orders, order items, and payments without duplicating existing seeded records.
