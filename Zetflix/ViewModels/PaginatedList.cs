using Microsoft.EntityFrameworkCore;

namespace Zetflix.ViewModels;

public class PaginatedList<T> : List<T>
{
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        PageSize = pageSize;

        AddRange(items);
    }

    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var safePageIndex = Math.Max(1, pageIndex);
        var items = await source
            .Skip((safePageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<T>(items, count, safePageIndex, pageSize);
    }
}
