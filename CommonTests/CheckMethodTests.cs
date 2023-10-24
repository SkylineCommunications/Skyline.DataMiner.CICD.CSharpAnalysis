namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.CSharpAnalysis;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    [TestClass]
    public class CheckMethodTests
    {
        [TestMethod]
        public void CheckMethod_Parameters()
        {
            List<string> results = new List<string>();

            const string programText =
   @"
public static class QAction
{
	public static void Method1() { }
	public static void Method2(string value) { }
	public static void Method3(string[] value) { }
	public static void Method4(ref uint value) { }
	public static void Method5(out double value) => value = 0;
	public static void Method6(params int[] value) { }
	public static void Method7(string value = ""ABC"") { }
}";
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            QActionAnalyzer analyzer = new QActionAnalyzer(CheckMethod);
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);

            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckMethod(MethodClass method)
            {
                results.Add(Method1(method));
                results.Add(Method2(method));
                results.Add(Method3(method));
                results.Add(Method4(method));
                results.Add(Method5(method));
                results.Add(Method6(method));
                results.Add(Method7(method));
            }

            string Method1(MethodClass method)
            {
                if (method.Name != "Method1")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().BeEmpty();
                }
                catch (AssertFailedException e)
                {
                    return $"[Method1] {e.Message}";
                }

                return String.Empty;
            }

            string Method2(MethodClass method)
            {
                if (method.Name != "Method2")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(1);
                    method.Parameters[0].IsOut.Should().BeFalse();
                    method.Parameters[0].IsParams.Should().BeFalse();
                    method.Parameters[0].IsRef.Should().BeFalse();
                    method.Parameters[0].Name.Should().BeEquivalentTo("value");
                    method.Parameters[0].Type.Should().BeEquivalentTo("string");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method2] {e.Message}";
                }

                return String.Empty;
            }

            string Method3(MethodClass method)
            {
                if (method.Name != "Method3")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(1);
                    method.Parameters[0].IsOut.Should().BeFalse();
                    method.Parameters[0].IsParams.Should().BeFalse();
                    method.Parameters[0].IsRef.Should().BeFalse();
                    method.Parameters[0].Name.Should().BeEquivalentTo("value");
                    method.Parameters[0].Type.Should().BeEquivalentTo("string[]");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method3] {e.Message}";
                }

                return String.Empty;
            }

            string Method4(MethodClass method)
            {
                if (method.Name != "Method4")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(1);
                    method.Parameters[0].IsOut.Should().BeFalse();
                    method.Parameters[0].IsParams.Should().BeFalse();
                    method.Parameters[0].IsRef.Should().BeTrue();
                    method.Parameters[0].Name.Should().BeEquivalentTo("value");
                    method.Parameters[0].Type.Should().BeEquivalentTo("uint");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method4] {e.Message}";
                }

                return String.Empty;
            }

            string Method5(MethodClass method)
            {
                if (method.Name != "Method5")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(1);
                    method.Parameters[0].IsOut.Should().BeTrue();
                    method.Parameters[0].IsParams.Should().BeFalse();
                    method.Parameters[0].IsRef.Should().BeFalse();
                    method.Parameters[0].Name.Should().BeEquivalentTo("value");
                    method.Parameters[0].Type.Should().BeEquivalentTo("double");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method5] {e.Message}";
                }

                return String.Empty;
            }

            string Method6(MethodClass method)
            {
                if (method.Name != "Method6")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(1);
                    method.Parameters[0].IsOut.Should().BeFalse();
                    method.Parameters[0].IsParams.Should().BeTrue();
                    method.Parameters[0].IsRef.Should().BeFalse();
                    method.Parameters[0].Name.Should().BeEquivalentTo("value");
                    method.Parameters[0].Type.Should().BeEquivalentTo("int[]");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method6] {e.Message}";
                }

                return String.Empty;
            }

            string Method7(MethodClass method)
            {
                if (method.Name != "Method7")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(1);
                    method.Parameters[0].IsOut.Should().BeFalse();
                    method.Parameters[0].IsParams.Should().BeFalse();
                    method.Parameters[0].IsRef.Should().BeFalse();
                    method.Parameters[0].Name.Should().BeEquivalentTo("value");
                    method.Parameters[0].Type.Should().BeEquivalentTo("string");
                    method.Parameters[0].DefaultValue.Should().NotBeNull();
                    method.Parameters[0].DefaultValue.Object.Should().BeEquivalentTo("ABC");
                    method.Parameters[0].DefaultValue.Type.Should().Be(Value.ValueType.String);
                }
                catch (AssertFailedException e)
                {
                    return $"[Method7] {e.Message}";
                }

                return String.Empty;
            }
        }

        [TestMethod]
        public void CheckMethod_GeneralInfo()
        {
            List<string> results = new List<string>();

            const string programText =
   @"
public class QAction
{
	public static void Method1() { }
	protected private static string Method2(string value, int value1, double value2) => "";
}";
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            QActionAnalyzer analyzer = new QActionAnalyzer(CheckMethod);
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckMethod(MethodClass method)
            {
                results.Add(Method1(method));
                results.Add(Method2(method));
            }

            string Method1(MethodClass method)
            {
                if (method.Name != "Method1")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().BeEmpty();
                    method.Access.Should().Be(AccessModifier.Public);
                    method.IsStatic.Should().BeTrue();
                    method.IsSealed.Should().BeFalse();
                    method.IsAbstract.Should().BeFalse();
                    method.IsNew.Should().BeFalse();
                    method.IsOverride.Should().BeFalse();
                    method.IsVirtual.Should().BeFalse();
                    method.ReturnType.Should().BeEquivalentTo("void");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method1] {e.Message}";
                }

                return String.Empty;
            }

            string Method2(MethodClass method)
            {
                if (method.Name != "Method2")
                {
                    return null;
                }

                try
                {
                    method.Parameters.Should().HaveCount(3);
                    method.Access.Should().Be(AccessModifier.Protected | AccessModifier.Private);
                    method.IsStatic.Should().BeTrue();
                    method.IsSealed.Should().BeFalse();
                    method.IsAbstract.Should().BeFalse();
                    method.IsNew.Should().BeFalse();
                    method.IsOverride.Should().BeFalse();
                    method.IsVirtual.Should().BeFalse();
                    method.ReturnType.Should().BeEquivalentTo("string");
                }
                catch (AssertFailedException e)
                {
                    return $"[Method2] {e.Message}";
                }

                return String.Empty;
            }
        }

        private class QActionAnalyzer : CSharpAnalyzerBase
        {
            private readonly Action<MethodClass> check;

            public QActionAnalyzer(Action<MethodClass> check)
            {
                this.check = check;
            }

            public override void CheckMethod(MethodClass method)
            {
                check.Invoke(method);
            }
        }
    }
}