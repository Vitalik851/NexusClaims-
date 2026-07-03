namespace ClaimsModule.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IEnumerable<string> Errors { get; }
    public IEnumerable<string> Warnings { get; }

    protected Result(bool isSuccess, T? value, IEnumerable<string> errors, IEnumerable<string> warnings)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? Array.Empty<string>();
        Warnings = warnings ?? Array.Empty<string>();
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>(), Array.Empty<string>());
    
    public static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors, Array.Empty<string>());
    public static Result<T> Failure(string error) => new(false, default, new[] { error }, Array.Empty<string>());

    public static Result<T> WithWarnings(T value, IEnumerable<string> warnings) => new(true, value, Array.Empty<string>(), warnings);
}
