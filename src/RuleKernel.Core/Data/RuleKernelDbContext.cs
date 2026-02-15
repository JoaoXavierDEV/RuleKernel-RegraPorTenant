using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Models;

namespace RuleKernel.Core.Data;

public sealed class RuleKernelDbContext : DbContext
{
    private static readonly Guid TenantSalomeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantRonyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid RuleDefinitionDataDeVencimentoId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid RuleDefinitionTaxaDeJurosId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public RuleKernelDbContext(DbContextOptions<RuleKernelDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleDefinition> RuleDefinitions => Set<RuleDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();

            entity.HasData(
                new Tenant { Id = TenantSalomeId, Name = "salome", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tenant { Id = TenantRonyId, Name = "rony", IsActive = true, CreatedAt = DateTime.UtcNow });
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RuleDefinitionId).IsRequired();
            entity.Property(r => r.SourceCode).IsRequired();
            entity.HasIndex(r => new { r.TenantId, r.RuleDefinitionId, r.Priority }).IsUnique();

            entity.HasOne(r => r.Tenant)
                .WithMany(t => t.Rules)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.RuleDefinition)
                .WithMany()
                .HasForeignKey(r => r.RuleDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RuleDefinition>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.ContractType).HasMaxLength(300).IsRequired();
            entity.HasIndex(r => new { r.Name, r.ContractType }).IsUnique();

            entity.HasData(
                new RuleDefinition
                {
                    Id = RuleDefinitionDataDeVencimentoId,
                    Name = "DataDeVencimento",
                    Description = "Contrato: DataDeVencimentoContract",
                    ContractType = "RuleKernel.Contract.DataDeVencimentoContract",
                    CreatedAt = DateTime.UtcNow
                },
                new RuleDefinition
                {
                    Id = RuleDefinitionTaxaDeJurosId,
                    Name = "TaxaDeJuros",
                    Description = "Contrato: TaxaDeJurosContract",
                    ContractType = "RuleKernel.Contract.TaxaDeJurosContract",
                    CreatedAt = DateTime.UtcNow
                });
        });
    }
}
