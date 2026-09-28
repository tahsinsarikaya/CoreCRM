using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.Redis;

namespace CoreCRM.Tests.Infrastructure
{
    public class RedisFixture : IAsyncLifetime
    {
        private readonly RedisContainer _container = new RedisBuilder()
            .WithImage("redis:7")
            .Build();

        private IConnectionMultiplexer? _connection;

        public IConnectionMultiplexer Connection => _connection
            ?? throw new InvalidOperationException("Redis connection not initialized yet.");

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            _connection = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        }

        public async Task DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();

            await _container.DisposeAsync();
        }
    }
}
