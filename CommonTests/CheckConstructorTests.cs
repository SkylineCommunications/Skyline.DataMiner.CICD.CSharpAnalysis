namespace Skyline.DataMiner.CICD.CSharpAnalysis.Tests
{
    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;
    using Skyline.DataMiner.CICD.CSharpAnalysis;

    [TestClass]
    public class CheckConstructorTests
    {
        [TestMethod]
        public void CheckConstructorStuff()
        {
            ConstructorAnalyzer analyzer = new ConstructorAnalyzer();
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
        private string testField;

        public Program(int var1, string var2, out List<int> var3)
        {
            var3 = new List<int>();
        }

        public string TestProperty { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            parser.Visit(tree.GetRoot());
        }

        internal class ConstructorAnalyzer : CSharpAnalyzerBase
        {
            public override void CheckConstructor(ConstructorClass constructor)
            {
                constructor.Name.Should().BeEquivalentTo("Program");
                constructor.Access.Should().BeEquivalentTo(AccessModifier.Public);
                constructor.Parameters.Should().HaveCount(3);

                //constructor.Parameters[0].GetType(semanticModel).Should().BeEquivalentTo(Value.ValueType.Int32);
                //constructor.Parameters[1].Type.Should().BeEquivalentTo("string");
                //constructor.Parameters[2].Type.Should().BeEquivalentTo("List<int>");

                constructor.Parameters[0].Name.Should().BeEquivalentTo("var1");
                constructor.Parameters[1].Name.Should().BeEquivalentTo("var2");
                constructor.Parameters[2].Name.Should().BeEquivalentTo("var3");

                constructor.Parameters[2].IsOut.Should().BeTrue();
            }
        }
    }
}
