using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public static class ApiEndpoints
{
    public static void MapRestaurantApiEndpoints(this WebApplication app)
    {
        app.MapCrudEndpoints("/api/users", "Users", db => db.Users, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/user-addresses", "User Addresses", db => db.UserAddresses, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/restaurants", "Restaurants", db => db.Restaurants, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/menu-items", "Menu Items", db => db.MenuItems, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/delivery-types", "Delivery Types", db => db.DeliveryTypes, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/orders", "Orders", db => db.Orders, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/payments", "Payments", db => db.Payments, entity => entity.Id, (entity, id) => entity.Id = id);
        app.MapCrudEndpoints("/api/order-items", "Order Items", db => db.OrderItems, entity => entity.Id, (entity, id) => entity.Id = id);
    }

    private static void MapCrudEndpoints<TEntity>(
        this WebApplication app,
        string route,
        string tag,
        Func<Db, DbSet<TEntity>> getSet,
        Func<TEntity, int> getId,
        Action<TEntity, int> setId)
        where TEntity : class
    {
        var group = app.MapGroup(route)
            .WithTags(tag);

        group.MapGet("/", async (Db db) =>
            await getSet(db)
                .AsNoTracking()
                .ToListAsync());

        group.MapGet("/{id:int}", async Task<Results<Ok<TEntity>, NotFound>> (int id, Db db) =>
        {
            var entity = await getSet(db).FindAsync(id);
            return entity is null ? TypedResults.NotFound() : TypedResults.Ok(entity);
        });

        group.MapPost("/", async (TEntity entity, Db db) =>
        {
            getSet(db).Add(entity);
            await db.SaveChangesAsync();

            return TypedResults.Created($"{route}/{getId(entity)}", entity);
        });

        group.MapPut("/{id:int}", async Task<Results<NoContent, BadRequest, NotFound>> (int id, TEntity entity, Db db) =>
        {
            if (getId(entity) != 0 && getId(entity) != id)
            {
                return TypedResults.BadRequest();
            }

            if (!await getSet(db).AnyAsync(existing => EF.Property<int>(existing, "Id") == id))
            {
                return TypedResults.NotFound();
            }

            setId(entity, id);
            db.Entry(entity).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        });

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, Db db) =>
        {
            var entity = await getSet(db).FindAsync(id);
            if (entity is null)
            {
                return TypedResults.NotFound();
            }

            getSet(db).Remove(entity);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        });
    }
}
