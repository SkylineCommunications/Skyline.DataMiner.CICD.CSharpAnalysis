namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class CSharpAnalyzerTests
    {
        [TestMethod()]
#pragma warning disable S2699
        public void CheckCommentLineTest()
#pragma warning restore S2699
        {
            CommentAnalyzer analyzer = new CommentAnalyzer();
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
        static void Main(string[] args)
        {
            // This is a test.
            Console.WriteLine(""Hello, World!"");
        }
    }
}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            parser.Visit(tree.GetRoot());
        }

        internal class CommentAnalyzer : CSharpAnalyzerBase
        {
            public override void CheckCommentLine(string commentLine, SyntaxTrivia syntaxTrivia)
            {
                commentLine.Should().BeEquivalentTo(" This is a test.");
            }
        }
    }
}