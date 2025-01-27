namespace Skyline.DataMiner.CICD.CSharpAnalysis
{
    using System;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

    /// <summary>
    /// Roslyn visitor.
    /// </summary>
    public class RoslynVisitor : CSharpSyntaxWalker
    {
        /*
         * IMPORTANT: Make sure that at the end of each Visit method, you do the base.Visit...
         * Otherwise it won't traverse correctly over the tree or even not at all.
         */

        private readonly CSharpAnalyzerBase analyzer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynVisitor"/> class. By default it will visit each node.
        /// </summary>
        /// <param name="analyzer">The C# analyzer.</param>
        /// <param name="depth">The depth to where it needs to look to.</param>
        public RoslynVisitor(CSharpAnalyzerBase analyzer, SyntaxWalkerDepth depth = SyntaxWalkerDepth.StructuredTrivia) : base(depth)
        {
            this.analyzer = analyzer;
        }

        /// <summary>
        /// Visits the node.
        /// </summary>
        /// <param name="node">The node.</param>
        public override void Visit(SyntaxNode node)
        {
            if (!analyzer.IsCanceled)
            {
                base.Visit(node);
            }
        }

        /// <summary>
        /// Visits the assignment expression node.
        /// </summary>
        /// <param name="node">The assignment expression node.</param>
        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            AssignmentExpressionClass assignmentExpression = AssignmentExpressionClass.Parse(node);

            analyzer.CheckAssignmentExpression(assignmentExpression);

            base.VisitAssignmentExpression(node);
        }

        /// <summary>
        /// Visits the trivia.
        /// </summary>
        /// <param name="trivia">The trivia.</param>
        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                    analyzer.CheckCommentLine(trivia.ToString().TrimStart('/'), trivia);
                    break;

                case SyntaxKind.MultiLineCommentTrivia:
                    var comment = trivia.ToString();
                    comment = comment.Substring(2, comment.Length - 4).Trim();

                    var lines = comment.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        analyzer.CheckCommentLine(line, trivia);
                    }

                    break;

            }

            base.VisitTrivia(trivia);
        }

        /// <summary>
        /// Visits the #if preprocessor directive.
        /// </summary>
        /// <param name="node">The #if preprocessor directive.</param>
        public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            if (node.Condition is IdentifierNameSyntax ins)
            {
                analyzer.CheckIfDirective(ins.Identifier.Text, node);
            }

            base.VisitIfDirectiveTrivia(node);
        }

        /// <summary>
        /// Visits the #define preprocessor directive.
        /// </summary>
        /// <param name="node">The #define preprocessor directive.</param>
        public override void VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
        {
            analyzer.CheckDefineDirective(node.Name.Text, node);
            base.VisitDefineDirectiveTrivia(node);
        }

        /// <summary>
        /// Visits the interface declaration syntax node.
        /// </summary>
        /// <param name="node">The interface declaration syntax node.</param>
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            InterfaceClass @interface = InterfaceClass.Parse(node);

            analyzer.CheckInterface(@interface);
            base.VisitInterfaceDeclaration(node);
        }

        /// <summary>
        /// Visits the class declaration node.
        /// </summary>
        /// <param name="node">The class declaration node.</param>
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ClassClass @class = ClassClass.Parse(node);

            // Run method
            analyzer.CheckClass(@class);
            base.VisitClassDeclaration(node);
        }

        /// <summary>
        /// Visits the method declaration node.
        /// </summary>
        /// <param name="node">The method declaration node.</param>
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            MethodClass method = MethodClass.Parse(node);

            // Run method
            analyzer.CheckMethod(method);
            base.VisitMethodDeclaration(node);
        }

        /// <summary>
        /// Visits the field declaration node.
        /// </summary>
        /// <param name="node">The field declaration node.</param>
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            FieldClass field = FieldClass.Parse(node);

            // Run method
            analyzer.CheckField(field);
            base.VisitFieldDeclaration(node);
        }

        /// <summary>
        /// Visits the property declaration node.
        /// </summary>
        /// <param name="node">The property declaration node.</param>
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            PropertyClass prop = PropertyClass.Parse(node);

            // Run method
            analyzer.CheckProperty(prop);
            base.VisitPropertyDeclaration(node);
        }


        /// <summary>
        /// Visits the invocation expression node.
        /// </summary>
        /// <param name="node">The invocation expression node.</param>
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            CallingMethodClass callingMethod = CallingMethodClass.Parse(node);

            // Run method
            analyzer.CheckCallingMethod(callingMethod);
            base.VisitInvocationExpression(node);
        }

        /// <summary>
        /// Visits the constructor declaration node.
        /// </summary>
        /// <param name="node">The constructor declaration node.</param>
        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            ConstructorClass ctor = ConstructorClass.Parse(node);

            analyzer.CheckConstructor(ctor);
            base.VisitConstructorDeclaration(node);
        }

        /// <summary>
        /// Visits the object creation expression node.
        /// </summary>
        /// <param name="node">The object creation expression node.</param>
        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            ObjectCreationClass occ = ObjectCreationClass.Parse(node);

            analyzer.CheckObjectCreation(occ);
            base.VisitObjectCreationExpression(node);
        }

        /// <summary>
        /// Visits the destructor declaration node.
        /// </summary>
        /// <param name="node">The destructor declaration node.</param>
        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            FinalizerClass fc = FinalizerClass.Parse(node);

            analyzer.CheckFinalizer(fc);
            base.VisitDestructorDeclaration(node);
        }
    }
}