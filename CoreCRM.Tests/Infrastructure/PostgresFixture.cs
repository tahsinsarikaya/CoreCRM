using CoreCRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace CoreCRM.Tests.Infrastructure
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("corecrm_test")
            .WithUsername("corecrm")
            .WithPassword("corecrm")
            .Build();

        public string ConnectionString => _container.GetConnectionString();

        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<CoreCrmDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            await using var dbContext = new CoreCrmDbContext(options);
            await dbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync() => await _container.DisposeAsync();

        public CoreCrmDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<CoreCrmDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            return new CoreCrmDbContext(options);
        }
    }
}
