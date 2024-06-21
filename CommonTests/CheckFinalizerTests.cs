namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.CSharpAnalysis;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

    [TestClass]
    public class CheckFinalizerTests
    {
        [TestMethod]
#pragma warning disable S2699
        public void CheckFinalizerStuff()
#pragma warning restore S2699
        {
            FinalizerAnalyzer analyzer = new FinalizerAnalyzer();
            RoslynVisitor parser = new RoslynVisitor(analyzer);

            const string programText =
   @"using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    internal sealed class Program
    {
        ~Program() { }
    }
}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            parser.Visit(tree.GetRoot());
        }

        private class FinalizerAnalyzer : CSharpAnalyzerBase
        {
            public override void CheckFinalizer(FinalizerClass finalizer)
            {
                finalizer.Name.Should().BeEquivalentTo("Program");
            }
        }
    }
}
