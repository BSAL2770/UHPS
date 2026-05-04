using Microsoft.EntityFrameworkCore;

namespace UHPS.API.Common;

public static class QueryableExtensions
{
    public const int MaxPageSize = 100;

    public static async Task<PagedResult<TOut>> ToPagedResultAsync<TIn, TOut>(
        this IQueryable<TIn> source,
        PagingQuery paging,
        Func<TIn, TOut> mapper,
        CancellationToken ct)
    {
        var page = paging.Page < 1 ? 1 : paging.Page;
        var pageSize = Math.Clamp(paging.PageSize, 1, MaxPageSize);

        var totalCount = await source.CountAsync(ct);
        var entities = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<TOut>
        {
            Items = entities.Select(mapper).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
