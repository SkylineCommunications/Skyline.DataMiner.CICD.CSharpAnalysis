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

    [TestClass]
    public class CheckFieldTests
    {
        [TestMethod]
        public void CheckField_Attributes()
        {
            List<string?> results = new List<string?>();

            const string programText =
   @"
public static class QAction
{
    [Description(""Field 1's custom description"")]
	public string field1;
    [Required]
	public Guid field2;
    [Required]
    [Description(""Field 3's custom description"")]
	public List<string> field3 = new List<string>();
	public int field4;
}";
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            QActionAnalyzer analyzer = new QActionAnalyzer(CheckFields);
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);

            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckFields(FieldClass field)
            {
                results.Add(Field1(field));
                results.Add(Field2(field));
                results.Add(Field3(field));
                results.Add(Field4(field));
            }

            string? Field1(FieldClass field)
            {
                if (field.Name != "field1")
                {
                    return null;
                }

                try
                {
                    field.Attributes.Should().HaveCount(1);
                    var descriptionAttribute = field.Attributes.Single(attribute => attribute.Name == "Description");
                    descriptionAttribute.Arguments.Should().NotBeNullOrEmpty();
                    descriptionAttribute.Arguments.Should().HaveCount(1);
                }
                catch (AssertFailedException e)
                {
                    return $"[field1] {e.Message}";
                }

                return String.Empty;
            }

            string? Field2(FieldClass field)
            {
                if (field.Name != "field2")
                {
                    return null;
                }

                try
                {
                    field.Attributes.Should().HaveCount(1);
                    var requiredAttribute = field.Attributes.Single(attribute => attribute.Name == "Required");
                    requiredAttribute.Arguments.Should().HaveCount(0);
                }
                catch (AssertFailedException e)
                {
                    return $"[field2] {e.Message}";
                }

                return String.Empty;
            }

            string? Field3(FieldClass field)
            {
                if (field.Name != "field3")
                {
                    return null;
                }

                try
                {
                    field.Attributes.Should().HaveCount(2);
                    var descriptionAttribute = field.Attributes.Single(attribute => attribute.Name == "Description");
                    descriptionAttribute.Arguments.Should().NotBeNullOrEmpty();
                    descriptionAttribute.Arguments.Should().HaveCount(1);

                    var requiredAttribute = field.Attributes.Single(attribute => attribute.Name == "Required");
                    requiredAttribute.Arguments.Should().HaveCount(0);
                }
                catch (AssertFailedException e)
                {
                    return $"[field3] {e.Message}";
                }

                return String.Empty;
            }

            string? Field4(FieldClass field)
            {
                if (field.Name != "field4")
                {
                    return null;
                }

                try
                {
                    field.Attributes.Should().HaveCount(0);
                }
                catch (AssertFailedException e)
                {
                    return $"[field4] {e.Message}";
                }

                return String.Empty;
            }
        }

        private class QActionAnalyzer : CSharpAnalyzerBase
        {
            private readonly Action<FieldClass> check;

            public QActionAnalyzer(Action<FieldClass> check)
            {
                this.check = check;
            }

            public override void CheckField(FieldClass field)
            {
                check.Invoke(field);
            }

        }
    }
}