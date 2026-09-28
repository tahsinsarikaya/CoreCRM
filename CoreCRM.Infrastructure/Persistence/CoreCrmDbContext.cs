using CoreCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Infrastructure.Persistence
{
    public class CoreCrmDbContext : DbContext
    {
        public CoreCrmDbContext(DbContextOptions<CoreCrmDbContext> options) : base(options) { }

        public DbSet<EntityDefinition> EntityDefinitions => Set<EntityDefinition>();
        public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();
        public DbSet<EntityRecord> EntityRecords => Set<EntityRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreCrmDbContext).Assembly);
        }
    }
}
