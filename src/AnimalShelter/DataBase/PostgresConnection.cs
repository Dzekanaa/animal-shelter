using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AnimalShelter.Database;

public static class PostgresConnection
{
    private static readonly string ConnectionString;

    static PostgresConnection()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var config = builder.Build();
        ConnectionString = config.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public static IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}