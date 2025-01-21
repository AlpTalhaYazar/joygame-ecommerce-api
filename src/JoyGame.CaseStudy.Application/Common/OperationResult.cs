namespace JoyGame.CaseStudy.Application.Common;

public class OperationResult
{
    public bool IsSuccess { get; protected set; }
    public ErrorCode ErrorCode { get; protected set; } = ErrorCode.None;
    public string? ErrorMessage { get; protected set; }
    public List<string> ErrorDetails { get; protected set; } = new();

    public static OperationResult Success()
        => new() { IsSuccess = true };

    public static OperationResult Failure(ErrorCode code, string message)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };

    public static OperationResult Failure(ErrorCode code, string message, List<string> details)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message, ErrorDetails = details };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; protected set; }

    public static OperationResult<T> Success(T data)
        => new() { IsSuccess = true, Data = data };

    public new static OperationResult<T> Failure(ErrorCode code, string message)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };

    public new static OperationResult<T> Failure(ErrorCode code, string message, List<string> details)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message, ErrorDetails = details };
}