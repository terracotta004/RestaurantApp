BEGIN;

INSERT INTO "Users" ("Email", "Name", "Phone", "CreatedAt")
VALUES
    ('alex@example.com', 'Alex Morgan', '555-0101', NOW()),
    ('jamie@example.com', 'Jamie Rivera', '555-0102', NOW()),
    ('taylor@example.com', 'Taylor Chen', NULL, NOW())
ON CONFLICT ("Email") DO NOTHING;

INSERT INTO "DeliveryTypes" ("Name", "IsActive", "CreatedAt")
VALUES
    ('DoorDash', TRUE, NOW()),
    ('Uber Eats', TRUE, NOW()),
    ('In-house Delivery', TRUE, NOW())
ON CONFLICT ("Name") DO NOTHING;

INSERT INTO "Restaurants" ("Name", "Description", "Address", "Phone", "CreatedAt")
SELECT 'Pasta Palace', 'Fresh pasta, sauces, and classic Italian comfort food.', '120 Main St, Springfield, IL 62701', '555-0201', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Restaurants" WHERE "Name" = 'Pasta Palace');

INSERT INTO "Restaurants" ("Name", "Description", "Address", "Phone", "CreatedAt")
SELECT 'Taco Junction', 'Street-style tacos, bowls, and house-made salsas.', '45 Market Ave, Springfield, IL 62701', '555-0202', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Restaurants" WHERE "Name" = 'Taco Junction');

INSERT INTO "Restaurants" ("Name", "Description", "Address", "Phone", "CreatedAt")
SELECT 'Green Garden', 'Fresh salads, grain bowls, wraps, and smoothies.', '88 Oak Blvd, Springfield, IL 62704', '555-0203', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Restaurants" WHERE "Name" = 'Green Garden');

WITH source_data AS (
    SELECT * FROM (VALUES
        ('alex@example.com', 'Home', '742 Evergreen Terrace', NULL, 'Springfield', 'IL', '62701', TRUE),
        ('alex@example.com', 'Work', '100 Capitol Plaza', 'Suite 400', 'Springfield', 'IL', '62701', FALSE),
        ('jamie@example.com', 'Apartment', '19 River Road', 'Apt 3B', 'Springfield', 'IL', '62703', TRUE),
        ('taylor@example.com', 'Home', '230 Maple Street', NULL, 'Springfield', 'IL', '62704', TRUE)
    ) AS data("Email", "Label", "StreetAddress", "ApartmentSuite", "City", "State", "PostalCode", "IsDefault")
)
INSERT INTO "UserAddresses" ("UserId", "Label", "StreetAddress", "ApartmentSuite", "City", "State", "PostalCode", "IsDefault", "CreatedAt")
SELECT users."Id", source_data."Label", source_data."StreetAddress", source_data."ApartmentSuite", source_data."City", source_data."State", source_data."PostalCode", source_data."IsDefault", NOW()
FROM source_data
JOIN "Users" users ON users."Email" = source_data."Email"
WHERE NOT EXISTS (
    SELECT 1
    FROM "UserAddresses" existing
    WHERE existing."UserId" = users."Id"
      AND existing."StreetAddress" = source_data."StreetAddress"
      AND COALESCE(existing."ApartmentSuite", '') = COALESCE(source_data."ApartmentSuite", '')
);

WITH source_data AS (
    SELECT * FROM (VALUES
        ('Pasta Palace', 'Spaghetti Marinara', 'Spaghetti with tomato sauce, garlic, and basil.', 12.99, TRUE),
        ('Pasta Palace', 'Chicken Alfredo', 'Fettuccine with grilled chicken and parmesan cream sauce.', 16.50, TRUE),
        ('Pasta Palace', 'Garlic Bread', 'Toasted bread with garlic butter and herbs.', 5.25, TRUE),
        ('Taco Junction', 'Carnitas Tacos', 'Three corn tortillas with slow-cooked pork and salsa verde.', 11.75, TRUE),
        ('Taco Junction', 'Veggie Burrito', 'Rice, beans, peppers, onions, avocado, and pico de gallo.', 10.50, TRUE),
        ('Taco Junction', 'Chips and Queso', 'Tortilla chips with warm queso dip.', 6.00, TRUE),
        ('Green Garden', 'Harvest Bowl', 'Quinoa, roasted sweet potato, kale, chickpeas, and tahini.', 13.25, TRUE),
        ('Green Garden', 'Caesar Wrap', 'Romaine, parmesan, croutons, and Caesar dressing in a wrap.', 9.95, TRUE),
        ('Green Garden', 'Berry Smoothie', 'Mixed berries, banana, yogurt, and oat milk.', 6.75, TRUE)
    ) AS data("RestaurantName", "Name", "Description", "Price", "IsAvailable")
)
INSERT INTO "MenuItems" ("RestaurantId", "Name", "Description", "Price", "IsAvailable", "CreatedAt")
SELECT restaurants."Id", source_data."Name", source_data."Description", source_data."Price", source_data."IsAvailable", NOW()
FROM source_data
JOIN "Restaurants" restaurants ON restaurants."Name" = source_data."RestaurantName"
WHERE NOT EXISTS (
    SELECT 1
    FROM "MenuItems" existing
    WHERE existing."RestaurantId" = restaurants."Id"
      AND existing."Name" = source_data."Name"
);

WITH order_data AS (
    SELECT *
    FROM (VALUES
        ('alex@example.com', 'Pasta Palace', 'Home', 'DoorDash', 'delivery', 'delivered', 35.73, NOW() - INTERVAL '2 days', 'Leave at the front door.'),
        ('jamie@example.com', 'Taco Junction', NULL, NULL, 'pickup', 'ready', 18.75, NOW() - INTERVAL '1 day', NULL),
        ('taylor@example.com', 'Green Garden', 'Home', 'Uber Eats', 'delivery', 'preparing', 20.00, NOW(), 'Please include utensils.')
    ) AS data("Email", "RestaurantName", "AddressLabel", "DeliveryTypeName", "FulfillmentType", "Status", "TotalAmount", "OrderedAt", "Notes")
),
inserted_orders AS (
    INSERT INTO "Orders" ("UserId", "RestaurantId", "DeliveryAddressId", "DeliveryTypeId", "FulfillmentType", "Status", "TotalAmount", "OrderedAt", "Notes")
    SELECT users."Id",
           restaurants."Id",
           addresses."Id",
           delivery_types."Id",
           order_data."FulfillmentType",
           order_data."Status",
           order_data."TotalAmount",
           order_data."OrderedAt",
           order_data."Notes"
    FROM order_data
    JOIN "Users" users ON users."Email" = order_data."Email"
    JOIN "Restaurants" restaurants ON restaurants."Name" = order_data."RestaurantName"
    LEFT JOIN "UserAddresses" addresses
        ON addresses."UserId" = users."Id"
       AND addresses."Label" = order_data."AddressLabel"
    LEFT JOIN "DeliveryTypes" delivery_types ON delivery_types."Name" = order_data."DeliveryTypeName"
    WHERE NOT EXISTS (
        SELECT 1
        FROM "Orders" existing
        WHERE existing."UserId" = users."Id"
          AND existing."RestaurantId" = restaurants."Id"
          AND existing."FulfillmentType" = order_data."FulfillmentType"
          AND existing."Status" = order_data."Status"
          AND existing."TotalAmount" = order_data."TotalAmount"
          AND COALESCE(existing."Notes", '') = COALESCE(order_data."Notes", '')
    )
    RETURNING "Id"
)
SELECT COUNT(*) FROM inserted_orders;

WITH pasta_order AS (
    SELECT orders."Id"
    FROM "Orders" orders
    JOIN "Users" users ON users."Id" = orders."UserId"
    JOIN "Restaurants" restaurants ON restaurants."Id" = orders."RestaurantId"
    WHERE users."Email" = 'alex@example.com'
      AND restaurants."Name" = 'Pasta Palace'
      AND orders."FulfillmentType" = 'delivery'
    ORDER BY orders."Id"
    LIMIT 1
),
taco_order AS (
    SELECT orders."Id"
    FROM "Orders" orders
    JOIN "Users" users ON users."Id" = orders."UserId"
    JOIN "Restaurants" restaurants ON restaurants."Id" = orders."RestaurantId"
    WHERE users."Email" = 'jamie@example.com'
      AND restaurants."Name" = 'Taco Junction'
      AND orders."FulfillmentType" = 'pickup'
    ORDER BY orders."Id"
    LIMIT 1
),
garden_order AS (
    SELECT orders."Id"
    FROM "Orders" orders
    JOIN "Users" users ON users."Id" = orders."UserId"
    JOIN "Restaurants" restaurants ON restaurants."Id" = orders."RestaurantId"
    WHERE users."Email" = 'taylor@example.com'
      AND restaurants."Name" = 'Green Garden'
      AND orders."FulfillmentType" = 'delivery'
    ORDER BY orders."Id"
    LIMIT 1
),
source_data AS (
    SELECT pasta_order."Id" AS "OrderId", 'Spaghetti Marinara' AS "MenuItemName", 2 AS "Quantity", 12.99::numeric AS "UnitPrice", NULL::text AS "SpecialInstructions" FROM pasta_order
    UNION ALL
    SELECT pasta_order."Id", 'Garlic Bread', 1, 5.25::numeric, 'Extra crispy' FROM pasta_order
    UNION ALL
    SELECT taco_order."Id", 'Carnitas Tacos', 1, 11.75::numeric, NULL FROM taco_order
    UNION ALL
    SELECT taco_order."Id", 'Chips and Queso', 1, 6.00::numeric, NULL FROM taco_order
    UNION ALL
    SELECT garden_order."Id", 'Harvest Bowl', 1, 13.25::numeric, 'Dressing on the side' FROM garden_order
    UNION ALL
    SELECT garden_order."Id", 'Berry Smoothie', 1, 6.75::numeric, NULL FROM garden_order
)
INSERT INTO "OrderItems" ("OrderId", "MenuItemId", "Quantity", "UnitPrice", "SpecialInstructions")
SELECT source_data."OrderId", menu_items."Id", source_data."Quantity", source_data."UnitPrice", source_data."SpecialInstructions"
FROM source_data
JOIN "MenuItems" menu_items ON menu_items."Name" = source_data."MenuItemName"
WHERE NOT EXISTS (
    SELECT 1
    FROM "OrderItems" existing
    WHERE existing."OrderId" = source_data."OrderId"
      AND existing."MenuItemId" = menu_items."Id"
);

WITH source_data AS (
    SELECT *
    FROM (VALUES
        ('alex@example.com', 'Pasta Palace', 'credit_card', 'paid', 35.73, 'txn_seed_pasta_001', '4242', NOW() - INTERVAL '2 days'),
        ('jamie@example.com', 'Taco Junction', 'credit_card', 'paid', 18.75, 'txn_seed_taco_001', '1111', NOW() - INTERVAL '1 day'),
        ('taylor@example.com', 'Green Garden', 'credit_card', 'authorized', 20.00, 'txn_seed_garden_001', '1881', NULL)
    ) AS data("Email", "RestaurantName", "PaymentMethod", "PaymentStatus", "Amount", "ProviderTransactionId", "CardLastFour", "PaidAt")
)
INSERT INTO "Payments" ("OrderId", "PaymentMethod", "PaymentStatus", "Amount", "ProviderTransactionId", "CardLastFour", "PaidAt", "CreatedAt")
SELECT orders."Id",
       source_data."PaymentMethod",
       source_data."PaymentStatus",
       source_data."Amount",
       source_data."ProviderTransactionId",
       source_data."CardLastFour",
       source_data."PaidAt",
       NOW()
FROM source_data
JOIN "Users" users ON users."Email" = source_data."Email"
JOIN "Restaurants" restaurants ON restaurants."Name" = source_data."RestaurantName"
JOIN "Orders" orders ON orders."UserId" = users."Id" AND orders."RestaurantId" = restaurants."Id"
ON CONFLICT ("ProviderTransactionId") DO NOTHING;

COMMIT;
