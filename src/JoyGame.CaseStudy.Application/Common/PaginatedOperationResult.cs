namespace JoyGame.CaseStudy.Application.Common;

public class PaginatedOperationResult<T> : OperationResult<T>
{
    public PaginationMetadata Metadata { get; private set; } = null!;

    public static PaginatedOperationResult<T> Success(T data, PaginationMetadata metadata)
    {
        return new PaginatedOperationResult<T>
        {
            IsSuccess = true,
            Data = data,
            Metadata = metadata
        };
    }

    public class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}