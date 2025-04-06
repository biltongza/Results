using Results.Lib;

public static class ResultExample
{
    public static Result<OkResult<int>, ErrorResult> Divide(int a, int b)
    {
        if (b == 0)
        {
            return Result.Error("Cannot divide by zero!");
        }
        var res = a / b;
        return Result.Ok(res);
    }
}