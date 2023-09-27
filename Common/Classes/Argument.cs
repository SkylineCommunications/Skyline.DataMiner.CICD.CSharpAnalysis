namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents an argument.
    /// </summary>
    public class Argument : CSharpObject<ArgumentSyntax>
    {
        internal Argument(ArgumentSyntax node) : base(node)
        {
            RawValue = node?.ToString();
        }

        /// <summary>
        /// Gets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public string RawValue { get; }

        /// <summary>
        /// Gets the fully qualified name. Will be <see langword="null"/> in case of a <see langword="null"/>.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns>The fully qualified name or <see langword="null"/>.</returns>
        public string GetFullyQualifiedName(SemanticModel semanticModel)
        {
            ITypeSymbol argSymbol = semanticModel.GetTypeInfo(SyntaxNode.Expression).Type;
            SymbolDisplayFormat symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            return argSymbol?.ToDisplayString(symbolDisplayFormat);
        }

        /// <summary>
        /// Tries parsing the specified model to a value.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="value">Contains the value if the parsing succeeded, otherwise <see langword="null"/>.</param>
        /// <returns><c>true</c> if s was converted successfully; otherwise, <c>false</c>.</returns>
        public bool TryParseToValue(SemanticModel semanticModel, Solution solution, out Value value)
        {
            if (RoslynHelper.TryParseValue(SyntaxNode.Expression, semanticModel, solution, out Value v))
            {
                value = v;
                value.SyntaxNode = SyntaxNode;
                return true;
            }

            value = null;
            return false;
        }
    }
}