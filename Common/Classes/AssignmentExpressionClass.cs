namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System;
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents an assignment expression.
    /// </summary>
    public class AssignmentExpressionClass : CSharpObject<AssignmentExpressionSyntax>
    {
        private AssignmentExpressionClass(AssignmentExpressionSyntax node) : base(node)
        {
        }

        /// <summary>
        /// Gets the member name.
        /// </summary>
        /// <value>The member name.</value>
        public string MemberName { get; private set; }

        /// <summary>
        /// Gets the property path.
        /// </summary>
        /// <value>The property path.</value>
        public string PropertyPath { get; private set; }

        /// <summary>
        /// Will return the Fully Qualified Name of the Parent. Or null in case it couldn't be resolved.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        public string GetFullyQualifiedNameOfLeftOperand(SemanticModel semanticModel)
        {
            return RoslynHelper.GetFullyQualifiedName(semanticModel, SyntaxNode.Left);
        }

        internal static AssignmentExpressionClass Parse(AssignmentExpressionSyntax node)
        {
            var assignmentExpression = new AssignmentExpressionClass(node);

            if (node.Left is MemberAccessExpressionSyntax maes && maes.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                var propertyPath = new List<string>();

                GetPropertyPath(propertyPath, maes);

                if (propertyPath.Count > 0)
                {
                    propertyPath.RemoveAt(0);
                    propertyPath.Reverse();
                    assignmentExpression.PropertyPath = String.Join(".", propertyPath);
                }

                if (maes.Name is IdentifierNameSyntax identifierName)
                {
                    assignmentExpression.MemberName = identifierName.Identifier.Text;
                }
            }

            return assignmentExpression;
        }

        private static void GetPropertyPath(List<string> path, MemberAccessExpressionSyntax maes)
        {
            if (maes.Expression is IdentifierNameSyntax identifierNameSyntax && maes.Name is IdentifierNameSyntax name)
            {
                path.Add(name.Identifier.Text);
                path.Add(identifierNameSyntax.Identifier.Text);
            }
            else if (maes.Expression is MemberAccessExpressionSyntax childMaes && maes.Name is IdentifierNameSyntax name2)
            {
                path.Add(name2.Identifier.Text);
                GetPropertyPath(path, childMaes);
            }
        }
    }
}