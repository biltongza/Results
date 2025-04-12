using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Results.Lib.Analyzers.Tests;

public class UnitTest1
{
    [Fact]
    public async Task R1000_Should_Work()
    {
        var loc = typeof(Result).Assembly.Location;
        var context = new CSharpAnalyzerTest<CheckAllResultTypes, DefaultVerifier>();
        context.ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddAssemblies(["Results.Lib", loc.Replace(".dll", string.Empty)]);
        context.CompilerDiagnostics = CompilerDiagnostics.Errors;
        context.TestCode = /* lang=c#-test */ """
            using System;
            using Results.Lib;
            namespace Something 
            {
                public class Program 
                {
                    public static void Main(string[] args) 
                    {
                        Result<OkResult, ErrorResult> result = Result.Ok();
                        _ = [|result.Actual switch {
                            OkResult ok => 1,
                            ErrorResult err => 2,
                            _ => throw new NotImplementedException()
                        }|];
                    }
                }
            }
        """;

        await context.RunAsync();
    }
}
