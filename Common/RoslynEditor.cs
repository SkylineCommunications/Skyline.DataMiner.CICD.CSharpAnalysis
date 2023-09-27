namespace SSkyline.DataMiner.CICD.CSharpAnalysis
{
    using System;
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

    /// <summary>
    /// Roslyn editor.
    /// </summary>
    internal static class RoslynEditor
    {
        /// <summary>
        /// Tries to resolve the <paramref name="value"/> into a literal expression.
        /// Can return null when failed to resolve.
        /// </summary>
        public static LiteralExpressionSyntax ResolveAsLiteral(Value value)
        {
            if (/*!value.HasStaticValue || */value.IsMethodArgument)
            {
                return null;
            }

            if (value.Type == Value.ValueType.Array || value.Array != null)
            {
                // Sometimes (casting) the Type is Object even though it's an array => Array is a filled in list.
                return null;
            }

            if (value.Object == null)
            {
                // Extra safety check
                return null;
            }

            if (value.Type == Value.ValueType.Boolean)
            {
                // Boolean can't have the literalArgument.
                if ((bool)value.Object)
                {
                    return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
                }

                return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
            }

            dynamic literalArgument = SyntaxFactory.Literal((dynamic)value.Object);

            switch (value.Type)
            {
                case Value.ValueType.String:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, literalArgument);

                case Value.ValueType.Int8:
                case Value.ValueType.Int16:
                case Value.ValueType.Int32:
                case Value.ValueType.Int64:
                case Value.ValueType.UInt8:
                case Value.ValueType.UInt16:
                case Value.ValueType.UInt32:
                case Value.ValueType.UInt64:
                case Value.ValueType.Single:
                case Value.ValueType.Double:
                case Value.ValueType.Decimal:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, literalArgument);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts the value to an Argument with every part resolved (variables filled in).
        /// </summary>
        public static ArgumentSyntax ResolveAsArgument(Value value)
        {
            if (value == null || value.IsMethodArgument)
            {
                return null;
            }

            if (value.Type == Value.ValueType.Array)
            {
                if (value.Array == null)
                {
                    // This should not happen.
                    throw new InvalidOperationException("[ResolveAsArgument]: Array was null whilst it wasn't a method argument.");
                }

                List<ExpressionSyntax> innerArrayParts = new List<ExpressionSyntax>();
                foreach (var argValueInner in value.Array)
                {
                    // Recursive in case of nested arrays.
                    var expression = ResolveAsArgument(argValueInner);
                    if (expression == null)
                    {
                        return null;
                    }

                    innerArrayParts.Add(expression.Expression);
                }

                SeparatedSyntaxList<ExpressionSyntax> separatedSyntaxList = new SeparatedSyntaxList<ExpressionSyntax>();
                separatedSyntaxList = separatedSyntaxList.AddRange(innerArrayParts);

                var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, separatedSyntaxList);

                var arrayExpression = SyntaxFactory.ImplicitArrayCreationExpression(initializer);
                return SyntaxFactory.Argument(arrayExpression);
            }

            var literalExpression = ResolveAsLiteral(value);
            return literalExpression == null ? null : SyntaxFactory.Argument(literalExpression);
        }
    }
}