using Microsoft.EntityFrameworkCore;
using RuleKernel.Models;

namespace RuleKernel.Data;

public class RuleKernelDbContext : DbContext
{
    public RuleKernelDbContext(DbContextOptions<RuleKernelDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Rule> Rules => Set<Rule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(200).IsRequired();
            entity.Property(r => r.SourceCode).IsRequired();
            entity.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();

            entity.HasOne(r => r.Tenant)
                  .WithMany(t => t.Rules)
                  .HasForeignKey(r => r.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
