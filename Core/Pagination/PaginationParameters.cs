namespace DD.Core.Pagination;
public abstract class PaginationParameters
{
    internal virtual int MaxPageSize { get; } = 25;
    internal virtual int DefaultPageSize { get; set; } = 25;
    public virtual int Page { get; set; } = 1;
    public int PageSize
    {
        get
        {
            return DefaultPageSize;
        }
        set
        {
            DefaultPageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
