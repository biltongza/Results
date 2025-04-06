using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Results.Apis;
public static class ResultApi
{
    public static RouteGroupBuilder MapResultApi(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("results");

        group.MapGet("", Divide);

        return builder;
    }

    private static Results<Ok<int>, BadRequest<string>> Divide([FromQuery] int a, [FromQuery] int b)
    {
        var result = ResultExample.Divide(a, b);

        return result.Actual switch
        {
            OkResult<int> ok => TypedResults.Ok(ok.Value),
            ErrorResult err => TypedResults.BadRequest(err.Message),
            _ => throw new NotImplementedException()
        };
    }
}