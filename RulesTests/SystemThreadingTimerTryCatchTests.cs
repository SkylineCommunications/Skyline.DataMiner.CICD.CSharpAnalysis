using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Skyline.DataMiner.CICD.CSharpAnalysis.Rules;
using Xunit;

namespace RulesTests
{

    using VerifyCS = CSharpAnalyzerVerifier<SystemThreadingTimerTryCatch, DefaultVerifier>;

    public class SystemThreadingTimerTryCatchTests
    {
        [Fact]
        public async Task LambdaWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(_ =>
        {
            Console.WriteLine(""Hello""); // missing try/catch
        });
    }
}";

            var expected = VerifyCS.Diagnostic("TIMER001")
                                   .WithSpan(10, 9, 12, 10);

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task LambdaWithTryCatch_ShouldNotReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(_ =>
        {
            try
            {
                Console.WriteLine(""Hello"");
            }
            catch {}
        });
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task MethodCallbackWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(Method);
    }

    void Method(object state)
    {
        Console.WriteLine(""Hello""); // missing try/catch
    }
}";

            var expected = VerifyCS.Diagnostic("TIMER001")
                                   .WithSpan(9, 27, 9, 33);

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task MethodCallbackWithTryCatch_ShouldNotReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(Method);
    }

    void Method(object state)
    {
        try
        {
            Console.WriteLine(""Hello"");
        }
        catch {}
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }

}
