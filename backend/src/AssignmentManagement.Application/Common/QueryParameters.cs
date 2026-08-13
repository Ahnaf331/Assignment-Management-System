namespace AssignmentManagement.Application.Common;

/// <summary>Common paging + search parameters accepted by list endpoints.</summary>
public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > MaxPageSize ? 10 : value;
    }

    public string? Search { get; set; }
}
