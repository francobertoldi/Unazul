using Microsoft.EntityFrameworkCore;
using SA.Operations.Domain.Entities;

namespace SA.Operations.DataAccess.EntityFramework.Persistence;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options) : DbContext(options)
{
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<ApplicantContact> ApplicantContacts => Set<ApplicantContact>();
    public DbSet<ApplicantAddress> ApplicantAddresses => Set<ApplicantAddress>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<ApplicationObservation> ApplicationObservations => Set<ApplicationObservation>();
    public DbSet<TraceEvent> TraceEvents => Set<TraceEvent>();
    public DbSet<TraceEventDetail> TraceEventDetails => Set<TraceEventDetail>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<SettlementItem> SettlementItems => Set<SettlementItem>();
    public DbSet<SettlementTotal> SettlementTotals => Set<SettlementTotal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OperationsDbContext).Assembly);
    }
}
