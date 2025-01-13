namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.CSharpAnalysis;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;
    using Attribute = Skyline.DataMiner.CICD.CSharpAnalysis.Classes.Attribute;

    [TestClass]
    public class CheckClassTests
    {
        [TestMethod]
        public void CheckClass_AccessModifiers()
        {
            List<string?> results = new List<string?>();

            const string programText =
                   @"
public class Class1
{
}

internal class Class2
{
}

public class Class3
{
	public class InnerClass1
	{
	}

	internal class InnerClass2
	{
	}

	protected class InnerClass3
	{
	}

	private class InnerClass4
	{
	}

	protected private class InnerClass5
	{
	}

	protected internal class InnerClass6
	{
	}
}";
            QActionAnalyzer analyzer = new QActionAnalyzer(CheckClass);
            RoslynVisitor parser = new RoslynVisitor(analyzer);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckClass(ClassClass classClass)
            {
                results.Add(Class1(classClass));
                results.Add(Class2(classClass));
                results.Add(Class3(classClass));
            }

            string? Class1(ClassClass classClass)
            {
                if (classClass.Name != "Class1")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Public);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class1] {e.Message}";
                }

                return String.Empty;
            }

            string? Class2(ClassClass classClass)
            {
                if (classClass.Name != "Class2")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Internal);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class2] {e.Message}";
                }

                return String.Empty;
            }

            string? Class3(ClassClass classClass)
            {
                if (classClass.Name != "Class3")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Public);
                    classClass.NestedClasses.Should().HaveCount(6);

                    results.Add(InnerClass1(classClass.NestedClasses[0]));
                    results.Add(InnerClass2(classClass.NestedClasses[1]));
                    results.Add(InnerClass3(classClass.NestedClasses[2]));
                    results.Add(InnerClass4(classClass.NestedClasses[3]));
                    results.Add(InnerClass5(classClass.NestedClasses[4]));
                    results.Add(InnerClass6(classClass.NestedClasses[5]));
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass1(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass1")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Public);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3|InnerClass1] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass2(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass2")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Internal);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3|InnerClass2] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass3(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass3")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Protected);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3|InnerClass3] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass4(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass4")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Private);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3|InnerClass4] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass5(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass5")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Protected | AccessModifier.Private);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3|InnerClass5] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass6(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass6")
                {
                    return null;
                }

                try
                {
                    classClass.Access.Should().Be(AccessModifier.Protected | AccessModifier.Internal);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3|InnerClass6] {e.Message}";
                }

                return String.Empty;
            }
        }

        [TestMethod]
        public void CheckClass_Keywords()
        {
            List<string?> results = new List<string?>();

            const string programText =
                   @"
public class Class1
{
}

public static class Class2
{
}

public abstract class Class3
{
}

public partial class Class4
{
}

public sealed class Class5
{
}

public sealed partial class Class6
{
}

public abstract partial class Class7
{
}";
            
            QActionAnalyzer analyzer = new QActionAnalyzer(CheckClass);
            RoslynVisitor parser = new RoslynVisitor(analyzer);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckClass(ClassClass classClass)
            {
                results.Add(Class1(classClass));
                results.Add(Class2(classClass));
                results.Add(Class3(classClass));
                results.Add(Class4(classClass));
                results.Add(Class5(classClass));
                results.Add(Class6(classClass));
                results.Add(Class7(classClass));
            }

            string? Class1(ClassClass classClass)
            {
                if (classClass.Name != "Class1")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeFalse();
                    classClass.IsPartial.Should().BeFalse();
                    classClass.IsSealed.Should().BeFalse();
                    classClass.IsStatic.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class1] {e.Message}";
                }

                return String.Empty;
            }

            string? Class2(ClassClass classClass)
            {
                if (classClass.Name != "Class2")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeFalse();
                    classClass.IsPartial.Should().BeFalse();
                    classClass.IsSealed.Should().BeFalse();
                    classClass.IsStatic.Should().BeTrue();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class2] {e.Message}";
                }

                return String.Empty;
            }

            string? Class3(ClassClass classClass)
            {
                if (classClass.Name != "Class3")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeTrue();
                    classClass.IsPartial.Should().BeFalse();
                    classClass.IsSealed.Should().BeFalse();
                    classClass.IsStatic.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class3] {e.Message}";
                }

                return String.Empty;
            }

            string? Class4(ClassClass classClass)
            {
                if (classClass.Name != "Class4")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeFalse();
                    classClass.IsPartial.Should().BeTrue();
                    classClass.IsSealed.Should().BeFalse();
                    classClass.IsStatic.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class4] {e.Message}";
                }

                return String.Empty;
            }

            string? Class5(ClassClass classClass)
            {
                if (classClass.Name != "Class5")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeFalse();
                    classClass.IsPartial.Should().BeFalse();
                    classClass.IsSealed.Should().BeTrue();
                    classClass.IsStatic.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class5] {e.Message}";
                }

                return String.Empty;
            }

            string? Class6(ClassClass classClass)
            {
                if (classClass.Name != "Class6")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeFalse();
                    classClass.IsPartial.Should().BeTrue();
                    classClass.IsSealed.Should().BeTrue();
                    classClass.IsStatic.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class6] {e.Message}";
                }

                return String.Empty;
            }

            string? Class7(ClassClass classClass)
            {
                if (classClass.Name != "Class7")
                {
                    return null;
                }

                try
                {
                    classClass.IsAbstract.Should().BeTrue();
                    classClass.IsPartial.Should().BeTrue();
                    classClass.IsSealed.Should().BeFalse();
                    classClass.IsStatic.Should().BeFalse();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class7] {e.Message}";
                }

                return String.Empty;
            }
        }

        [TestMethod]
        public void CheckClass_Finalizer()
        {
            List<string?> results = new List<string?>();

            const string programText =
                   @"
public class Class1
{
}

public class Class2
{
    ~Class2() { }
}";

            QActionAnalyzer analyzer = new QActionAnalyzer(CheckClass);
            RoslynVisitor parser = new RoslynVisitor(analyzer);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckClass(ClassClass classClass)
            {
                results.Add(Class1(classClass));
                results.Add(Class2(classClass));
            }

            string? Class1(ClassClass classClass)
            {
                if (classClass.Name != "Class1")
                {
                    return null;
                }

                try
                {
                    classClass.Finalizer.Should().BeNull();
                }
                catch (AssertFailedException e)
                {
                    return $"[Class1] {e.Message}";
                }

                return String.Empty;
            }

            string? Class2(ClassClass classClass)
            {
                if (classClass.Name != "Class2")
                {
                    return null;
                }

                try
                {
                    classClass.Finalizer.Should().NotBeNull();
                    classClass.Finalizer.Name.Should().Be("Class2");
                }
                catch (AssertFailedException e)
                {
                    return $"[Class2] {e.Message}";
                }

                return String.Empty;
            }
        }

        [TestMethod]
        public void CheckClass_Attributes()
        {
            List<string?> results = new List<string?>();

            const string programText =
                   @"using System;

    [Class(""A"", ""B"", ""C"")]
    [PropertyName(MyPropertyName = ""MyName"")]
    [CustomEnum(CustomEnum.A)]
	[Other(""A, the first letter"", ""B, the second letter"", c:""C, the third letter"")]
    public class Class1
    {
    }

    public class ClassAttribute : Attribute
    {
	    public ClassAttribute(params string[] values)
	    {
	    }
    }

    public class PropertyNameAttribute : Attribute
    {
	    public string MyPropertyName { get; set; }
    }

    public class OtherAttribute : Attribute
    {
        public OtherAttribute(string a, string b = """", string c = """")
        {
            A = a;
            B = b;
            C = c;
        }

        internal string A { get; } = String.Empty;

        internal string B { get; } = String.Empty;

        internal string C { get; } = String.Empty;
    }

    public class CustomEnumAttribute : Attribute
    {
	    public CustomEnumAttribute(CustomEnum customEnum)
	    {
	    }
    }

    public enum CustomEnum
    {
        Unknown,
        A,
        B, 
        C
    }";
            var solution = SolutionBuilderHelper.CreateSolutionFromSource(programText);

            foreach (Project project in solution.Projects)
            {
                var semanticModel = SolutionBuilderHelper.BuildProject(project);

                var analyzer = new QActionAnalyzerWithSemanticModelAndSolution(semanticModel, CheckClass, solution);
                RoslynVisitor parser = new RoslynVisitor(analyzer);
                parser.Visit(semanticModel.SyntaxTree.GetRoot());

                results.RemoveAll(String.IsNullOrWhiteSpace);
            }

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckClass(ClassClass classClass, SemanticModel semanticModel, Solution solution)
            {
                results.Add(Class1(classClass, semanticModel, solution));
            }

            string? Class1(ClassClass classClass, SemanticModel semanticModel, Solution solution)
            {
                if (classClass.Name != "Class1")
                {
                    return null;
                }

                try
                {
                    classClass.Attributes.Should().HaveCount(4);

                    Attribute classAttribute = classClass.Attributes.Single(attribute => attribute.Name == "Class");
                    classAttribute.Arguments.Should().NotBeNullOrEmpty();
                    classAttribute.Arguments.Should().HaveCount(3);
                    foreach (AttributeArgument attributeArgument in classAttribute.Arguments)
                    {
                        bool tryParseToValue = attributeArgument.TryParseToValue(out Value value);
                        tryParseToValue.Should().BeTrue();
                        value.Should().NotBeNull();
                        value.Type.Should().Be(Value.ValueType.String);
                        value.AsString.Should().BeOneOf("A", "B", "C");
                    }

                    Attribute propertyNameAttribute = classClass.Attributes.Single(attribute => attribute.Name == "PropertyName");
                    propertyNameAttribute.Arguments.Should().NotBeNullOrEmpty();
                    propertyNameAttribute.Arguments.Should().HaveCount(1);
                    propertyNameAttribute.Arguments[0].Name.Should().BeEquivalentTo("MyPropertyName");
                    propertyNameAttribute.Arguments[0].TryParseToValue(out Value v).Should().BeTrue();
                    v.Should().NotBeNull();
                    v.Type.Should().Be(Value.ValueType.String);
                    v.AsString.Should().Be("MyName");

                    Attribute customEnumAttribute = classClass.Attributes.Single(attribute => attribute.Name == "CustomEnum");
                    customEnumAttribute.Arguments.Should().NotBeNullOrEmpty();
                    customEnumAttribute.Arguments.Should().HaveCount(1);

                    // No semantic model & solution
                    customEnumAttribute.Arguments[0].TryParseToValue(out Value v2).Should().BeFalse();

                    // With semantic model & solution
                    customEnumAttribute.Arguments[0].TryParseToValue(semanticModel, solution, out Value v3).Should().BeTrue();
                    v3.Should().NotBeNull();
                    v3.Type.Should().Be(Value.ValueType.Int32);
                    v3.AsInt32.Should().Be(1);

                    Attribute otherAttribute = classClass.Attributes.Single(attribute => attribute.Name == "Other");
                    otherAttribute.Arguments.Should().NotBeNullOrEmpty();
                    otherAttribute.Arguments.Should().HaveCount(3);

                    foreach (AttributeArgument attributeArgument in otherAttribute.Arguments)
                    {
                        bool tryParseToValue = attributeArgument.TryParseToValue(out Value value);
                        tryParseToValue.Should().BeTrue();
                        value.Should().NotBeNull();
                        value.Type.Should().Be(Value.ValueType.String);
                        value.AsString.Should().BeOneOf("A, the first letter", "B, the second letter", "C, the third letter");

                        if (value.AsString == "C, the third letter")
                        {
                            attributeArgument.Name.Should().Be("c");
                        }
                    }
                }
                catch (AssertFailedException e)
                {
                    return $"[Class1] {e.Message}";
                }

                return String.Empty;
            }
        }

        [TestMethod]
        public void CheckClass_Namespaces()
        {
            List<string?> results = new List<string?>();

            const string programText =
                   @"
namespace MyNamespace
{
    public class Class1
    {
        public class InnerClass1
        {
        }
    }
}


internal class Class2
{
}
";
            QActionAnalyzer analyzer = new QActionAnalyzer(CheckClass);
            RoslynVisitor parser = new RoslynVisitor(analyzer);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);
            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckClass(ClassClass classClass)
            {
                results.Add(Class1(classClass));
                results.Add(Class2(classClass));
            }

            string? Class1(ClassClass classClass)
            {
                if (classClass.Name != "Class1")
                {
                    return null;
                }

                try
                {
                    classClass.Namespace.Should().Be("MyNamespace");

                    results.Add(InnerClass1(classClass.NestedClasses[0]));
                }
                catch (AssertFailedException e)
                {
                    return $"[Class1] {e.Message}";
                }

                return String.Empty;
            }

            string? Class2(ClassClass classClass)
            {
                if (classClass.Name != "Class2")
                {
                    return null;
                }

                try
                {
                    classClass.Namespace.Should().Be(String.Empty);
                }
                catch (AssertFailedException e)
                {
                    return $"[Class2] {e.Message}";
                }

                return String.Empty;
            }

            string? InnerClass1(ClassClass classClass)
            {
                if (classClass.Name != "InnerClass1")
                {
                    return null;
                }

                try
                {
                    classClass.Namespace.Should().Be("MyNamespace");
                }
                catch (AssertFailedException e)
                {
                    return $"[Class1|InnerClass1] {e.Message}";
                }

                return String.Empty;
            }
        }

        private class QActionAnalyzer : CSharpAnalyzerBase
        {
            private readonly Action<ClassClass> check;

            public QActionAnalyzer(Action<ClassClass> check)
            {
                this.check = check;
            }

            public override void CheckClass(ClassClass classClass)
            {
                check.Invoke(classClass);
            }
        }

        private class QActionAnalyzerWithSemanticModelAndSolution : CSharpAnalyzerBase
        {
            private readonly SemanticModel semanticModel;
            private readonly Action<ClassClass, SemanticModel, Solution> check;
            private readonly Solution solution;

            public QActionAnalyzerWithSemanticModelAndSolution(SemanticModel semanticModel, Action<ClassClass, SemanticModel, Solution> check, Solution solution)
            {
                this.semanticModel = semanticModel;
                this.check = check;
                this.solution = solution;
            }

            public override void CheckClass(ClassClass classClass)
            {
                check.Invoke(classClass, semanticModel, solution);
            }
        }
    }
}