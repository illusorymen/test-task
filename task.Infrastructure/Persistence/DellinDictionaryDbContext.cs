using System.Reflection;
using Microsoft.EntityFrameworkCore;
using task.Domain.Entities;

namespace task.Infrastructure.Persistence;

public class DellinDictionaryDbContext : DbContext
{
    public DbSet<Office> Offices { get; set; }
    public DbSet<Phone> Phones { get; set; }

    public DellinDictionaryDbContext(DbContextOptions<DellinDictionaryDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
