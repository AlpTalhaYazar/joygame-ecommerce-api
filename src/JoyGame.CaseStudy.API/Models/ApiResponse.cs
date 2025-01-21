namespace JoyGame.CaseStudy.API.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ErrorDetails? Error { get; set; }

    public class ErrorDetails
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Details { get; set; }
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

public class PaginatedApiResponse<T> : ApiResponse<T>
{
    public PaginationMetadata? Pagination { get; set; }

    public class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}