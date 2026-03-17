namespace Shared.Pagination;

public sealed record PaginationRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string Order = "asc"
)
{
    public int Skip => (Page - 1) * PageSize;
    public int ClampedPageSize => Math.Clamp(PageSize, 1, 100);
}
