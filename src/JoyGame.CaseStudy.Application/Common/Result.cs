namespace JoyGame.CaseStudy.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T Data { get; private set; }
    public ResultError Error { get; private set; } = new ResultError();

    public class ResultError
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new List<string>();
    }

    public static Result<T> Success(T data)
    {
        return new Result<T> { IsSuccess = true, Data = data };
    }

    public static Result<T> Failure(string message)
    {
        return new Result<T> { IsSuccess = false, Error = new ResultError { Message = message } };
    }

    public static Result<T> Failure(string message, List<string> details)
    {
        return new Result<T> { IsSuccess = false, Error = new ResultError { Message = message, Details = details } };
    }
}