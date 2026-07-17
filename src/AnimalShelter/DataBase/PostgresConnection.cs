using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AnimalShelter.Database;

public static class PostgresConnection
{
    private static readonly string ConnectionString;

    static PostgresConnection()
    {
        var basePath = AppContext.BaseDirectory;
        var configPath = Path.Combine(basePath, "appsettings.json");

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found at: {configPath}");
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
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