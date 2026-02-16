using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Models;

namespace RuleKernel.Core.Data;

public sealed class RuleKernelDbContext : DbContext
{
    private static readonly Guid TenantSalomeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantRonyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid RuleDefinitionDataDeVencimentoId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid RuleDefinitionTaxaDeJurosId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid RuleDefinitionCalculoDescontoId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid RuleDefinitionSalomeDataDeVencimentoId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid RuleDefinitionRonyDataDeVencimentoId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid RuleDefinitionSalomeCalculoDescontoId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid RuleDefinitionRonyCalculoDescontoId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly Guid RuleSalomeDataDeVencimentoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid RuleRonyDataDeVencimentoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid RuleSalomeCalculoDescontoId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid RuleRonyCalculoDescontoId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public RuleKernelDbContext(DbContextOptions<RuleKernelDbContext> options) : base(options)
    {
        if (Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            Database.EnsureCreated();
        }
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleDefinition> RuleDefinitions => Set<RuleDefinition>();
    public DbSet<Fatura> Faturas => Set<Fatura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();

            entity.HasOne(t => t.RegraDataVencimento)
                .WithMany()
                .HasForeignKey(t => t.RegraDataVencimentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.RegraCalculoDesconto)
                .WithMany()
                .HasForeignKey(t => t.RegraCalculoDescontoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new Tenant { Id = TenantSalomeId, Name = "salome", IsActive = true, CreatedAt = DateTime.UtcNow.Date },
                new Tenant { Id = TenantRonyId, Name = "rony", IsActive = true, CreatedAt = DateTime.UtcNow.Date });
        });

        modelBuilder.Entity<Fatura>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.ValorPrincipal).HasPrecision(18, 2);
            entity.Property(f => f.TaxaAdministracao).HasPrecision(18, 2);
            entity.Property(f => f.Desconto).HasPrecision(18, 2);
            entity.Property(f => f.Juros).HasPrecision(18, 2);
            entity.Property(f => f.Multa).HasPrecision(18, 2);
            entity.Property(f => f.ValorTotal).HasPrecision(18, 2);

            entity.HasOne(f => f.Tenant)
                .WithMany(t => t.Faturas)
                .HasForeignKey(f => f.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RuleDefinitionId).IsRequired();
            entity.Property(r => r.SourceCode).IsRequired();
            entity.HasIndex(r => new { r.TenantId, r.RuleDefinitionId, r.Priority }).IsUnique();

            entity.HasOne(r => r.RuleDefinition)
                .WithMany()
                .HasForeignKey(r => r.RuleDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new Rule
                {
                    Id = RuleSalomeDataDeVencimentoId,
                    TenantId = TenantSalomeId,
                    RuleDefinitionId = RuleDefinitionSalomeDataDeVencimentoId,
                    Priority = 1,
                    IsActive = true,
                    SourceCode = @$"contract.OutDataVencimento = contract.InDataDeEmissao.Date.AddDays(7);
                                    Console.WriteLine(""SALOME_DataDeVencimento"");",
                },
                new Rule
                {
                    Id = RuleRonyDataDeVencimentoId,
                    TenantId = TenantRonyId,
                    RuleDefinitionId = RuleDefinitionRonyDataDeVencimentoId,
                    Priority = 1,
                    IsActive = true,
                    SourceCode = @$"contract.OutDataVencimento = contract.InDataDeEmissao.Date.AddDays(10);
                                    Console.WriteLine(""RONY_DataDeVencimento"");",
                },
                new Rule
                {
                    Id = RuleSalomeCalculoDescontoId,
                    TenantId = TenantSalomeId,
                    RuleDefinitionId = RuleDefinitionSalomeCalculoDescontoId,
                    Priority = 1,
                    IsActive = true,
                    SourceCode = @$"contract.OutPercentualDesconto = contract.InValorPrincipal >= 1000m 
                                    ? contract.InValorPrincipal * 0.05m : 
                                    0m;
                                    contract.OutResult = contract.InValorPrincipal - contract.OutPercentualDesconto;
                                    Console.WriteLine(""SALOME_CalculoDesconto"");",
                },
                new Rule
                {
                    Id = RuleRonyCalculoDescontoId,
                    TenantId = TenantRonyId,
                    RuleDefinitionId = RuleDefinitionRonyCalculoDescontoId,
                    Priority = 1,
                    IsActive = true,
                    SourceCode = @$"contract.OutPercentualDesconto = 0m;
                                    contract.OutResult = contract.InValorPrincipal;
                                    Console.WriteLine(""RONY_CalculoDesconto"");",
                });
        });

        modelBuilder.Entity<RuleDefinition>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.ContractType).HasMaxLength(300).IsRequired();
            entity.HasIndex(r => new { r.Name, r.ContractType }).IsUnique();

            entity.HasData(
                //new RuleDefinition
                //{
                //    Id = RuleDefinitionDataDeVencimentoId,
                //    Name = "DataDeVencimento",
                //    Description = "Cálculo personalizado da data de vencimento",
                //    ContractType = "RuleKernel.Contract.DataDeVencimentoContract",
                //    CreatedAt = DateTime.UtcNow.Date
                //},
                //new RuleDefinition
                //{
                //    Id = RuleDefinitionTaxaDeJurosId,
                //    Name = "TaxaDeJuros",
                //    Description = "Cálculo personalizado da taxa de juros",
                //    ContractType = "RuleKernel.Contract.TaxaDeJurosContract",
                //    CreatedAt = DateTime.UtcNow.Date
                //},
                //new RuleDefinition
                //{
                //    Id = RuleDefinitionCalculoDescontoId,
                //    Name = "CalculoDesconto",
                //    Description = "Cálculo personalizado do desconto",
                //    ContractType = "RuleKernel.Contract.CalculoDescontoContract",
                //    CreatedAt = DateTime.UtcNow.Date
                //},
                new RuleDefinition
                {
                    Id = RuleDefinitionSalomeDataDeVencimentoId,
                    Name = "SALOME_DataDeVencimento",
                    Description = "Regra de data de vencimento para Salome (seed)",
                    ContractType = "RuleKernel.Core.Contract.DataDeVencimentoContract",
                    CreatedAt = DateTime.UtcNow.Date
                },
                new RuleDefinition
                {
                    Id = RuleDefinitionRonyDataDeVencimentoId,
                    Name = "RONY_DataDeVencimento",
                    Description = "Regra de data de vencimento para Rony (seed)",
                    ContractType = "RuleKernel.Core.Contract.DataDeVencimentoContract",
                    CreatedAt = DateTime.UtcNow.Date
                },
                new RuleDefinition
                {
                    Id = RuleDefinitionSalomeCalculoDescontoId,
                    Name = "SALOME_CalculoDesconto",
                    Description = "Regra de cálculo de desconto para Salome (seed)",
                    ContractType = "RuleKernel.Core.Contract.CalculoDescontoContract",
                    CreatedAt = DateTime.UtcNow.Date
                },
                new RuleDefinition
                {
                    Id = RuleDefinitionRonyCalculoDescontoId,
                    Name = "RONY_CalculoDesconto",
                    Description = "Regra de cálculo de desconto para Rony (seed)",
                    ContractType = "RuleKernel.Core.Contract.CalculoDescontoContract",
                    CreatedAt = DateTime.UtcNow.Date
                }
                );

        });
    }
}
