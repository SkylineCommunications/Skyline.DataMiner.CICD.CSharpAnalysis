namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.CSharpAnalysis;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

    [TestClass]
    public class CheckCallingMethodTests
    {
        [TestMethod]
        public void CheckCallingMethod_TryParseValue_ReferencedVariables()
        {
            List<string> results = new List<string>();

            string precompileText = @"
namespace MyOtherNamespace
{
    using System.Collections.Generic;
    public static class MyOtherConstants
    {
        public static Queue<int> TriggerQueue = new Queue<int>();

		public const double MyDoubleConst = 123.45;
    }
}";

            string programText = @"
namespace MyNamespace
{
    using System;

    using MyOtherNamespace;
	//using Skyline.DataMiner.Scripting;

	public static class MyConstants
	{
		public const object MyNullConst = null;

		public const string MyStringConst = ""ABC"";

		public const sbyte MyInt8Const = 1;
		public const short MyInt16Const = 153;
		public const int MyInt32Const = 26654652;
		public const long MyInt64Const = 131351216813;

		public const byte MyUInt8Const = 1;
		public const ushort MyUInt16Const = 153;
		public const uint MyUInt32Const = 26654652;
		public const ulong MyUInt64Const = 131351216813;

		public const double MyDoubleConst = 153.51;
	}

	public static class OtherAssemblyClass
	{
		public const int Parameter1 = 1; //Parameter.myparam;
	}

	public class Data
	{
		public SubData Sub;

		public Data()
		{
			Sub = new SubData(0);
		}

		public static readonly object[] colUpdates = new object[] { 11000, 11002, 11003, 11004, 11005, 11006, 11007, 11008, 11009, 11010, 11011, 11012, 11013, 11014, 11015, 11016, 11017, 11018, 11019, 11020, 11021, 11022, 11023, 11024, 11025, 11026, 11027, 11028, 11029, 11030 };

		public class SubData
		{
			public int MyValue;

			public SubData(int abc)
			{
				MyValue = abc;
			}
		}
	}

    public static class QAction
    {
	    private static int c = 6;

	    public static void Run(int randomValue)
	    {
		    // Local variables
		    int a = 1;
		    Method1(a);
		    int b = 2;
		    b += randomValue;
		    Method2(b); // Not guaranteed constant

		    // Constants from another class/precompile
		    Method3(MyConstants.MyNullConst);

		    Method4(MyConstants.MyStringConst);

		    Method5(MyConstants.MyInt8Const);
		    Method6(MyConstants.MyInt16Const);
		    Method7(MyConstants.MyInt32Const);
		    Method8(MyConstants.MyInt64Const);

		    Method9(MyConstants.MyUInt8Const);
		    Method10(MyConstants.MyUInt16Const);
		    Method11(MyConstants.MyUInt32Const);
		    Method12(MyConstants.MyUInt64Const);

		    Method13(MyConstants.MyDoubleConst);

		    // Global variables
		    Method14(c);

		    int d = 5;
		    d++;
		    Method15(d);

            Method16(MyOtherConstants.TriggerQueue.Dequeue());
            Method17(MyOtherConstants.MyDoubleConst);

            Method18(new DateTime(2008, 8, 29, 19, 27, 15));
            Method19(MyOtherConstants.TriggerQueue);
	    }

	    private static void Method1(object o) { }
	    private static void Method2(object o) { }
	    private static void Method3(object o) { }
	    private static void Method4(object o) { }
	    private static void Method5(object o) { }
	    private static void Method6(object o) { }
	    private static void Method7(object o) { }
	    private static void Method8(object o) { }
	    private static void Method9(object o) { }
	    private static void Method10(object o) { }
	    private static void Method11(object o) { }
	    private static void Method12(object o) { }
	    private static void Method13(object o) { }
	    private static void Method14(object o) { }
	    private static void Method15(object o) { }
        private static void Method16(object o) { }
        private static void Method17(object o) { }
        private static void Method18(object o) { }
        private static void Method19(object o) { }
    }
}";

            var solution = SolutionBuilderHelper.CreateSolutionFromSource(programText, precompileText);

            foreach (Project project in solution.Projects)
            {
                var semanticModel = SolutionBuilderHelper.BuildProject(project);

                QActionAnalyzer analyzer = new QActionAnalyzer(semanticModel, CheckCallingMethod, solution);
                RoslynVisitor parser = new RoslynVisitor(analyzer);
                parser.Visit(semanticModel.SyntaxTree.GetRoot());

                results.RemoveAll(String.IsNullOrWhiteSpace);
            }

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckCallingMethod(CallingMethodClass callingMethod, SemanticModel semanticModel,Solution solution)
            {
                results.Add(Method1(callingMethod, semanticModel, solution));
                results.Add(Method2(callingMethod, semanticModel, solution));

                results.Add(Method3(callingMethod, semanticModel, solution));

                results.Add(Method4(callingMethod, semanticModel, solution));

                results.Add(Method5(callingMethod, semanticModel, solution));
                results.Add(Method6(callingMethod, semanticModel, solution));
                results.Add(Method7(callingMethod, semanticModel, solution));
                results.Add(Method8(callingMethod, semanticModel, solution));

                results.Add(Method9(callingMethod, semanticModel, solution));
                results.Add(Method10(callingMethod, semanticModel, solution));
                results.Add(Method11(callingMethod, semanticModel, solution));
                results.Add(Method12(callingMethod, semanticModel, solution));

                results.Add(Method13(callingMethod, semanticModel, solution));

                results.Add(Method14(callingMethod, semanticModel, solution));

                results.Add(Method15(callingMethod, semanticModel, solution));

                // Using type from another project
                results.Add(Method16(callingMethod, semanticModel, solution));
                results.Add(Method17(callingMethod, semanticModel, solution));

                results.Add(Method18(callingMethod, semanticModel, solution));
                results.Add(Method19(callingMethod, semanticModel, solution));
            }

            string Method1(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method1")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(1);
                    value.Type.Should().Be(Value.ValueType.Int32);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method1] {e.Message}";
                }

                return String.Empty;
            }

            string Method2(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method2")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(2);
                    value.Type.Should().Be(Value.ValueType.Int32);
                    value.HasNotChanged.Should().BeFalse(); // Can't be guaranteed that it will be constant
                }
                catch (AssertFailedException e)
                {
                    return $"[Method2] {e.Message}";
                }

                return String.Empty;
            }

            string Method3(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method3")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo((object)null);
                    value.Type.Should().Be(Value.ValueType.Null);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method3] {e.Message}";
                }

                return String.Empty;
            }

            string Method4(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method4")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo("ABC");
                    value.Type.Should().Be(Value.ValueType.String);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method4] {e.Message}";
                }

                return String.Empty;
            }

            string Method5(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method5")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(1);
                    value.Type.Should().Be(Value.ValueType.Int8);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method5] {e.Message}";
                }

                return String.Empty;
            }

            string Method6(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method6")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(153);
                    value.Type.Should().Be(Value.ValueType.Int16);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method6] {e.Message}";
                }

                return String.Empty;
            }

            string Method7(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method7")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(26654652);
                    value.Type.Should().Be(Value.ValueType.Int32);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method7] {e.Message}";
                }

                return String.Empty;
            }

            string Method8(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method8")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(131351216813);
                    value.Type.Should().Be(Value.ValueType.Int64);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method8] {e.Message}";
                }

                return String.Empty;
            }

            string Method9(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method9")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(1);
                    value.Type.Should().Be(Value.ValueType.UInt8);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method9] {e.Message}";
                }

                return String.Empty;
            }

            string Method10(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method10")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(153);
                    value.Type.Should().Be(Value.ValueType.UInt16);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method10] {e.Message}";
                }

                return String.Empty;
            }

            string Method11(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method11")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(26654652);
                    value.Type.Should().Be(Value.ValueType.UInt32);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method11] {e.Message}";
                }

                return String.Empty;
            }

            string Method12(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method12")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(131351216813);
                    value.Type.Should().Be(Value.ValueType.UInt64);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method12] {e.Message}";
                }

                return String.Empty;
            }

            string Method13(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method13")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(153.51);
                    value.Type.Should().Be(Value.ValueType.Double);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method13] {e.Message}";
                }

                return String.Empty;
            }

            string Method14(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method14")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(6);
                    value.Type.Should().Be(Value.ValueType.Int32);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method14] {e.Message}";
                }

                return String.Empty;
            }

            string Method15(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method15")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(5);
                    value.Type.Should().Be(Value.ValueType.Int32);
                    value.HasNotChanged.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method15] {e.Message}";
                }

                return String.Empty;
            }

            string Method16(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method16")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method16] {e.Message}";
                }

                return String.Empty;
            }

            string Method17(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method17")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Object.Should().BeEquivalentTo(123.45);
                    value.Type.Should().Be(Value.ValueType.Double);
                    value.HasNotChanged.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method17] {e.Message}";
                }

                return String.Empty;
            }

            string Method18(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method18")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Type.Should().Be(Value.ValueType.Unknown);
                    value.Object.Should().BeEquivalentTo("System.DateTime");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method18] {e.Message}";
                }

                return String.Empty;
            }

            string Method19(CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution)
            {
                if (callingMethod.Name != "Method19")
                {
                    return null;
                }

                try
                {
                    callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value).Should().BeTrue();
                    value.Type.Should().Be(Value.ValueType.Unknown);
                    value.Object.Should().BeEquivalentTo("new Queue<int>()");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method19] {e.Message}";
                }

                return String.Empty;
            }
        }

        private class QActionAnalyzer : CSharpAnalyzerBase
        {
            private readonly SemanticModel semanticModel;
            private readonly Action<CallingMethodClass, SemanticModel, Solution> check;
            private readonly Solution solution;

            public QActionAnalyzer(SemanticModel semanticModel, Action<CallingMethodClass, SemanticModel, Solution> check, Solution solution)
            {
                this.semanticModel = semanticModel;
                this.check = check;
                this.solution = solution;
            }

            public override void CheckCallingMethod(CallingMethodClass callingMethod)
            {
                check.Invoke(callingMethod, semanticModel, solution);
            }
        }
    }
}