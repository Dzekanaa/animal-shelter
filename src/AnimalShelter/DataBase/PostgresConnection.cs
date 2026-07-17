using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using AnimalShelter.Models.Enums;
using Npgsql.NameTranslation;
using Dapper;

namespace AnimalShelter.Database;

public static class PostgresConnection
{
    private static readonly NpgsqlDataSource DataSource;

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
        var connectionString = config.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        

        DataSource = dataSourceBuilder.Build();
        
        

    }

    public static IDbConnection CreateConnection()
    {
        return DataSource.OpenConnection();
    }
}