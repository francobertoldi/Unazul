using Microsoft.EntityFrameworkCore;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;

namespace SA.Operations.DataAccess.EntityFramework.Repositories;

public sealed class ApplicationRepository(OperationsDbContext db) : IApplicationRepository
{
    public async Task<Application?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Applications.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Application?> GetByIdWithApplicantAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Applications
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Application> Items, int Total)> ListAsync(
        int skip,
        int take,
        string? search = null,
        ApplicationStatus? status = null,
        Guid? entityId = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var query = db.Applications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var matchingApplicantIds = db.Applicants
                .Where(ap =>
                    EF.Functions.ILike(ap.FirstName, $"%{s}%") ||
                    EF.Functions.ILike(ap.LastName, $"%{s}%") ||
                    EF.Functions.ILike(ap.DocumentNumber, $"%{s}%"))
                .Select(ap => ap.Id);

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, $"%{s}%") ||
                matchingApplicantIds.Contains(x.ApplicantId));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        var total = await query.CountAsync(ct);

        var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "code" => isDescending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "status" => isDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "created_at" => isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updated_at" => isDescending ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
        };

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Applications.AnyAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(Application application, CancellationToken ct = default)
    {
        await db.Applications.AddAsync(application, ct);
    }

    public void Update(Application application)
    {
        db.Applications.Update(application);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetNextSequenceAsync(Guid tenantId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SOL-{year}-";

        var maxCode = await db.Applications
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(prefix))
            .Select(x => x.Code)
            .MaxAsync(ct)
            .ConfigureAwait(false);

        if (maxCode is null)
            return 1;

        var numericPart = maxCode[prefix.Length..];
        return int.TryParse(numericPart, out var current) ? current + 1 : 1;
    }

    public async Task<int> TransitionStatusAsync(
        Guid id,
        ApplicationStatus currentStatus,
        ApplicationStatus newStatus,
        Guid updatedBy,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var currentStatusStr = currentStatus.ToString();
        var newStatusStr = newStatus.ToString();

        return await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE applications
               SET status = {newStatusStr},
                   updated_at = {now},
                   updated_by = {updatedBy}
               WHERE id = {id}
                 AND status = {currentStatusStr}",
            ct);
    }

    public async Task<IReadOnlyList<Application>> GetApprovedByDateRangeAsync(
        Guid tenantId,
        Guid? entityId,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct = default)
    {
        var query = db.Applications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Status == ApplicationStatus.Approved
                && x.UpdatedAt >= dateFrom
                && x.UpdatedAt <= dateTo);

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        return await query
            .OrderBy(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> BatchTransitionToSettledAsync(
        IReadOnlyList<Guid> ids,
        Guid updatedBy,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var approvedStr = ApplicationStatus.Approved.ToString();
        var settledStr = ApplicationStatus.Settled.ToString();

        return await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE applications
               SET status = {settledStr},
                   updated_at = {now},
                   updated_by = {updatedBy}
               WHERE id = ANY({ids.ToArray()})
                 AND status = {approvedStr}",
            ct);
    }
}
