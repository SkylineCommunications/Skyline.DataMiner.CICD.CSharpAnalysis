namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.CSharpAnalysis;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;
    
    [TestClass]
    public class CheckInterfaceTests
    {
        [TestMethod]
#pragma warning disable S2699
        public void CheckInterfaceStuff()
#pragma warning restore S2699
        {
            InterfaceAnalyzer analyzer = new InterfaceAnalyzer();
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            const string programText =
   @"using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    public interface Program
    {
        string TestProperty { get; set; }

        void TestMethod();
    }
}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            parser.Visit(tree.GetRoot());
        }

        private class InterfaceAnalyzer : CSharpAnalyzerBase
        {
            public override void CheckInterface(InterfaceClass @interface)
            {
                @interface.Access.Should().Be(AccessModifier.Public);
                @interface.Name.Should().BeEquivalentTo("Program");
                @interface.Properties.Should().HaveCount(1);
                @interface.Methods.Should().HaveCount(1);
                @interface.IsPartial.Should().BeFalse();

                var propertySyntax = SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), "TestProperty");

                PropertyClass property = new PropertyClass(propertySyntax)
                {
                    Access = AccessModifier.None,
                    IsGetter = true,
                    IsSetter = true,
                    Name = "TestProperty",
                    Type = "string",
                };
                @interface.Properties[0].Should().BeEquivalentTo(property, x => x.Excluding(y => y.SyntaxNode));
            }
        }
    }
}