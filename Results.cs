public interface IResult;

public static class Result
{
    public static OkResult Ok() => new();
    public static OkResult<T> Ok<T>(T value) => new(value);
    public static ErrorResult Error(string message) => new(message);
    public static ErrorResult<T> Error<T>(string message, T value) => new(message, value);
}

public record Result<TResult1, TResult2>
    where TResult1 : IResult
    where TResult2 : IResult
{
    public IResult Actual { get; }
    public Result(IResult result)
    {
        Actual = result;
    }

    public static implicit operator Result<TResult1, TResult2>(TResult1 result) => new((IResult)result);
    public static implicit operator Result<TResult1, TResult2>(TResult2 result) => new((IResult)result);
}

public record OkResult : IResult;
public record OkResult<T>(T Value) : OkResult;

public record ErrorResult(string Message) : IResult;
public record ErrorResult<T>(string Message, T Value) : ErrorResult(Message);