using Npgsql;

public static class SeedDataRunner
{
    public static async Task RunAsync(string connectionString, string seedFilePath)
    {
        var sql = await File.ReadAllTextAsync(seedFilePath);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = 60
        };

        await command.ExecuteNonQueryAsync();
    }
}
