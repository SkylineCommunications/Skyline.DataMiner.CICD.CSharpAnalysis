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

    [TestClass]
    public class CheckPropertyTests
    {
        [TestMethod]
        public void CheckProperty_Attributes()
        {
            List<string?> results = new List<string?>();

            const string programText =
   @"
public static class QAction
{
    [Description(""Property 1's custom description"")]
	public string Prop1 { get; set; }
    [Required]
	public Guid Prop2 { get; set; }
    [Required]
    [Description(""Property 3's custom description"")]
	public List<string> Prop3 { get; set; } = new List<string>();
	public int Prop4 { get; }
}";
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(programText);

            QActionAnalyzer analyzer = new QActionAnalyzer(CheckProperties);
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            parser.Visit(syntaxTree.GetRoot());

            results.RemoveAll(String.IsNullOrWhiteSpace);

            if (results.Count > 0)
            {
                throw new AssertFailedException(String.Join(Environment.NewLine, results));
            }

            void CheckProperties(PropertyClass property)
            {
                results.Add(Property1(property));
                results.Add(Property2(property));
                results.Add(Property3(property));
                results.Add(Property4(property));
            }

            string? Property1(PropertyClass property)
            {
                if (property.Name != "Prop1")
                {
                    return null;
                }

                try
                {
                    property.Attributes.Should().HaveCount(1);
                    var descriptionAttribute = property.Attributes.Single(attribute => attribute.Name == "Description");
                    descriptionAttribute.Arguments.Should().NotBeNullOrEmpty();
                    descriptionAttribute.Arguments.Should().HaveCount(1);
                }
                catch (AssertFailedException e)
                {
                    return $"[Prop1] {e.Message}";
                }

                return String.Empty;
            }

            string? Property2(PropertyClass property)
            {
                if (property.Name != "Prop2")
                {
                    return null;
                }

                try
                {
                    property.Attributes.Should().HaveCount(1);
                    var requiredAttribute = property.Attributes.Single(attribute => attribute.Name == "Required");
                    requiredAttribute.Arguments.Should().HaveCount(0);
                }
                catch (AssertFailedException e)
                {
                    return $"[Prop2] {e.Message}";
                }

                return String.Empty;
            }

            string? Property3(PropertyClass property)
            {
                if (property.Name != "Prop3")
                {
                    return null;
                }

                try
                {
                    property.Attributes.Should().HaveCount(2);
                    var descriptionAttribute = property.Attributes.Single(attribute => attribute.Name == "Description");
                    descriptionAttribute.Arguments.Should().NotBeNullOrEmpty();
                    descriptionAttribute.Arguments.Should().HaveCount(1);

                    var requiredAttribute = property.Attributes.Single(attribute => attribute.Name == "Required");
                    requiredAttribute.Arguments.Should().HaveCount(0);
                }
                catch (AssertFailedException e)
                {
                    return $"[Prop3] {e.Message}";
                }

                return String.Empty;
            }

            string? Property4(PropertyClass property)
            {
                if (property.Name != "Prop4")
                {
                    return null;
                }

                try
                {
                    property.Attributes.Should().HaveCount(0);
                }
                catch (AssertFailedException e)
                {
                    return $"[Prop4] {e.Message}";
                }

                return String.Empty;
            }
        }

        private class QActionAnalyzer : CSharpAnalyzerBase
        {
            private readonly Action<PropertyClass> check;

            public QActionAnalyzer(Action<PropertyClass> check)
            {
                this.check = check;
            }

            public override void CheckProperty(PropertyClass propertyClass)
            {
                check.Invoke(propertyClass);
            }
        }
    }
}