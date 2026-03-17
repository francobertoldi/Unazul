using Microsoft.EntityFrameworkCore;
using SA.Catalog.Domain.Entities;

namespace SA.Catalog.DataAccess.EntityFramework.Persistence;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<ProductFamily> ProductFamilies => Set<ProductFamily>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPlan> ProductPlans => Set<ProductPlan>();
    public DbSet<PlanLoanAttributes> PlanLoanAttributes => Set<PlanLoanAttributes>();
    public DbSet<PlanInsuranceAttributes> PlanInsuranceAttributes => Set<PlanInsuranceAttributes>();
    public DbSet<PlanAccountAttributes> PlanAccountAttributes => Set<PlanAccountAttributes>();
    public DbSet<PlanCardAttributes> PlanCardAttributes => Set<PlanCardAttributes>();
    public DbSet<PlanInvestmentAttributes> PlanInvestmentAttributes => Set<PlanInvestmentAttributes>();
    public DbSet<Coverage> Coverages => Set<Coverage>();
    public DbSet<ProductRequirement> ProductRequirements => Set<ProductRequirement>();
    public DbSet<CommissionPlan> CommissionPlans => Set<CommissionPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
