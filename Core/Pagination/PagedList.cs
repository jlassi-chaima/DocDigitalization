namespace DD.Core.Pagination;
public class PagedList<T>
{
    public IList<T> Results { get; }
    public PagedList(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize, List<Guid> guids)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        count = totalItems;
        All = guids.ToList();
        if (totalItems > 0)
        {
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }
        Results = items as IList<T> ?? new List<T>(items);
    }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int count { get; }
    public List<Guid> All { get; }
    public bool IsFirstPage => PageNumber == 1;
    public bool IsLastPage => PageNumber == TotalPages && TotalPages > 0;
}