namespace ProductService.Application.Common;

public class Results<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int? StatusCode { get; }

    private Results(bool isSuccess, T? value, string? error, int? statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public static Results<T> Success(T value, int statusCode = 200) 
        => new(true, value, null, statusCode);

    public static Results<T> Failure(string error, int statusCode = 400) 
        => new(false, default, error, statusCode);
}
