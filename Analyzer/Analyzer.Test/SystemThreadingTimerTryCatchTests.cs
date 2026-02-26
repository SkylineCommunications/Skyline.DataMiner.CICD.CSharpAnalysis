namespace Analyzer.Test
{
    using System.Threading.Tasks;

    using Xunit;

    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Skyline.DataMiner.CICD.CSharpAnalysis.Analyzer.SystemThreadingTimerTryCatch, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

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
        var t = new Timer({|#0:_ =>
        {
            Console.WriteLine(""Hello""); // missing try/catch
        }|});
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
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
        var t = new Timer({|#0:Method|});
    }

    void Method(object state)
    {
        Console.WriteLine(""Hello""); // missing try/catch
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
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

        // -------- Additional callback forms (all using location) --------

        [Fact]
        public async Task ExpressionBodiedLambdaWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer({|#0:_ => Console.WriteLine(""Hello"")|});
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task AnonymousMethodWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer({|#0:delegate(object state)
        {
            Console.WriteLine(""Hello"");
        }|});
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task AnonymousMethodWithTryCatch_ShouldNotReportDiagnostic()
        {
            var test = @"
using System;
            using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(delegate(object state)
        {
            try
            {
                Console.WriteLine(""Hello"");
            }
            catch
            {
            }
        });
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task MemberAccessMethodGroup_InstanceWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer({|#0:this.Callback|});
    }

    void Callback(object state)
    {
        Console.WriteLine(""Hello""); // missing try/catch
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task MemberAccessMethodGroup_StaticWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

static class Helper
{
    public static void StaticCallback(object state)
    {
        Console.WriteLine(""Hello""); // missing try/catch
    }
}

class TestClass
{
    void Test()
    {
        var t = new Timer({|#0:Helper.StaticCallback|});
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task MemberAccessMethodGroup_InstanceWithTryCatch_ShouldNotReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(this.SafeCallback);
    }

    void SafeCallback(object state)
    {
        try
        {
            Console.WriteLine(""Hello"");
        }
        catch
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NamedArguments_LambdaWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(
            callback: {|#0:_ => Console.WriteLine(""Hello"")|},
            state: null,
            dueTime: 0,
            period: 1000);
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task NamedArguments_LambdaWithTryCatch_ShouldNotReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(
            callback: _ =>
            {
                try
                {
                    Console.WriteLine(""Hello"");
                }
                catch
                {
                }
            },
            state: null,
            dueTime: 0,
            period: 1000);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NamedArguments_MethodGroupWithoutTryCatch_ShouldReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(
            callback: {|#0:Method|},
            state: null,
            dueTime: 0,
            period: 1000);
    }

    void Method(object state)
    {
        Console.WriteLine(""Hello""); // missing try/catch
    }
}";
            var expected = VerifyCS.Diagnostic("TIMER001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task NamedArguments_AnonymousMethodWithTryCatch_ShouldNotReportDiagnostic()
        {
            var test = @"
using System;
using System.Threading;

class TestClass
{
    void Test()
    {
        var t = new Timer(
            callback: delegate(object state)
            {
                try
                {
                    Console.WriteLine(""Hello"");
                }
                catch
                {
                }
            },
            state: null,
            dueTime: 0,
            period: 1000);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}