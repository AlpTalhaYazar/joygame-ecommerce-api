namespace JoyGame.CaseStudy.Application.Common;

public class PaginationResult<T> : Result<T>
{
    public int Page { get; private set; }
    public int Limit { get; private set; }
    public int Total { get; private set; }
    public int TotalPages { get; private set; }
    public bool HasNext { get; private set; }
    public bool HasPrevious { get; private set; }

    public static PaginationResult<T> Success(T data, int page, int limit, int total)
    {
        var totalPages = (int)Math.Ceiling(total / (double)limit);
        return new PaginationResult<T>
        {
            IsSuccess = true,
            Data = data,
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            HasPrevious = page > 1
        };
    }
}