﻿using System;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExampleWeb.UnitTests;

public class UnitTestWithCustomSeedData : IDisposable
{
    private readonly DbContextOptions<ExampleDbContext> _dbContextOptions;
    private readonly NpgsqlDatabaseInitializer _databaseInitializer;

    public UnitTestWithCustomSeedData()
    {
        _databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434, }
        );
        _dbContextOptions = _databaseInitializer
            .CreateDatabaseGetDbContextOptionsBuilderSync<ExampleDbContext>(
                new DatabaseSeedingOptions<ExampleDbContext>(
                    "DatabaseWithSeedData",
                    async context =>
                    {
                        context.Users.Add(new User() { Id = 5, Name = "Eugene" });
                        await context.SaveChangesAsync();
                    }
                )
            )
            .Options;
    }

    public ExampleDbContext CreateDbContext()
    {
        return new ExampleDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Test1()
    {
        var service = new UserService(CreateDbContext());
        var users = await service.GetUsers();
        Assert.Equal(new[] { "John", "Bill", "Eugene" }, users);
    }

    public void Dispose()
    {
        _databaseInitializer?.RemoveDatabase(CreateDbContext().Database.GetConnectionString());
    }
}
