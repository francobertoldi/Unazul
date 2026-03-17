using Microsoft.EntityFrameworkCore;

namespace Shared.Pagination;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Skip)
            .Take(request.ClampedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, total, request.Page, request.ClampedPageSize);
    }
}
