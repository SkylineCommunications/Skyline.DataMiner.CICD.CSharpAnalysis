namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents an argument.
    /// </summary>
    public class AttributeArgument : CSharpObject<AttributeArgumentSyntax>
    {
        internal AttributeArgument(AttributeArgumentSyntax node) : base(node)
        {
            Name = node?.NameEquals?.Name.Identifier.Text;
            RawValue = node?.Expression.ToFullString();
        }

        /// <summary>
        /// Gets the name of the argument. Only filled in when using [Attribute(ArgName = ...)].
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the raw value of the argument as a string.
        /// </summary>
        public string RawValue { get; }

        /// <summary>
        /// Tries parsing to a value.
        /// </summary>
        /// <param name="value">Contains the value if the parsing succeeded, otherwise <see langword="null"/>.</param>
        /// <returns><c>true</c> if s was converted successfully; otherwise, <c>false</c>.</returns>
        public bool TryParseToValue(out Value value)
        {
            if (RoslynHelper.TryParseValue(SyntaxNode.Expression, out Value v))
            {
                value = v;
                value.SyntaxNode = SyntaxNode;
                return true;
            }

            value = null;
            return false;
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