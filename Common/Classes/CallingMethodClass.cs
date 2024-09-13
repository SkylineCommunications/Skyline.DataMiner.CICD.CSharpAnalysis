namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System;
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents a calling method.
    /// </summary>
    public class CallingMethodClass : CSharpObject<InvocationExpressionSyntax>
    {
        private CallingMethodClass(InvocationExpressionSyntax node) : base(node)
        {
            Arguments = new List<Argument>();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the parent name.
        /// </summary>
        /// <value>The parent name.</value>
        public string ParentName { get; internal set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public List<Argument> Arguments { get; }

        /// <summary>
        /// Returns the Fully Qualified Name of the Parent. Or null in case it could not be resolved.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        public string GetFullyQualifiedNameOfParent(SemanticModel semanticModel)
        {
            return RoslynHelper.GetFullyQualifiedName(semanticModel, SyntaxNode.Expression);
        }

        /// <summary>
        /// Returns the type symbol of the parent.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns>The type symbol of the parent or <see langword="null"/> in case it could not be resolved.</returns>
        public ITypeSymbol GetTypeSymbol(SemanticModel semanticModel)
        {
            return RoslynHelper.GetTypeSymbol(semanticModel, SyntaxNode.Expression);
        }

        /// <summary>
        /// Returns a value indicating whether the parent is of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns><c>true</c> if it is of the specified type; otherwise, <c>false</c>.</returns>
        public bool ParentIsOfType(Type type, SemanticModel semanticModel)
        {
            string fqn = GetFullyQualifiedNameOfParent(semanticModel);

            return type.FullName == fqn;
        }

        internal static CallingMethodClass Parse(InvocationExpressionSyntax node)
        {
            CallingMethodClass callingMethod = new CallingMethodClass(node);

            if (node.Expression is MemberAccessExpressionSyntax maes && maes.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                callingMethod.Name = maes.Name.Identifier.Text;

                if (maes.Expression is IdentifierNameSyntax ins)
                {
                    callingMethod.ParentName = ins.Identifier.Text;
                }
            }
            else if (node.Expression is IdentifierNameSyntax ins)
            {
                callingMethod.Name = ins.Identifier.Text;
            }

            foreach (var arg in node.ArgumentList?.Arguments ?? new SeparatedSyntaxList<ArgumentSyntax>())
            {
                callingMethod.Arguments.Add(new Argument(arg));
            }

            return callingMethod;
        }
    }
}