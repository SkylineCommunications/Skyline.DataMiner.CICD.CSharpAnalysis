namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.CSharpAnalysis;

    [TestClass]
    public class CheckDirectiveTests
    {
        [TestMethod]
        public void CheckDirectiveStuff()
        {
            QActionAnalyzer analyzer = new QActionAnalyzer();
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            const string programText =
   @"
#define BLA
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    internal sealed class Program
    {
        private string testField;

        public Program()
        {
        }

        internal string TestProperty { get; set; }

        public static List<int> TestProperty2 { get; }

        public virtual string[] TestProperty3 { get; }

        static void Main(string[] args)
        {
            #if ABC
            Console.WriteLine(""Hello, World!"");
            #endif
        }
    }
}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            parser.Visit(tree.GetRoot());
        }

        internal class QActionAnalyzer : CSharpAnalyzerBase
        {
            public override void CheckDefineDirective(string directiveName, DefineDirectiveTriviaSyntax directive)
            {
                directiveName.Should().BeEquivalentTo("BLA");
            }

            public override void CheckIfDirective(string directiveName, IfDirectiveTriviaSyntax directive)
            {
                directiveName.Should().BeEquivalentTo("ABC");
            }
        }
    }
}
