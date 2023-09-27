namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    
    /// <summary>
    /// Represents a parameter/argument.
    /// </summary>
    public class ParameterClass : CSharpObject<ParameterSyntax>
    {
        private ParameterClass(ParameterSyntax node) : base(node)
        {
        }

		/// <summary>
		/// Gets a value indicating whether this is an out parameter.
		/// </summary>
		/// <value><c>true</c> if this is an out parameter;otherwise, <c>false</c>.</value>
		public bool IsOut { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is a ref parameter.
		/// </summary>
		/// <value><c>true</c> if this is a ref parameter;otherwise, <c>false</c>.</value>
		public bool IsRef { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is a params parameter.
		/// </summary>
		/// <value><c>true</c> if this is a params parameter;otherwise, <c>false</c>.</value>
		public bool IsParams { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <value>The default value.</value>
        public Value DefaultValue { get; private set; }

        internal static ParameterClass Parse(ParameterSyntax node)
        {
            var p = new ParameterClass(node)
            {
                Name = node.Identifier.Text
            };

            foreach (SyntaxToken modifier in node.Modifiers)
            {
                switch (modifier.Kind())
                {
                    case SyntaxKind.OutKeyword:
                        p.IsOut = true;
                        break;

                    case SyntaxKind.RefKeyword:
                        p.IsRef = true;
                        break;

                    case SyntaxKind.ParamsKeyword:
                        p.IsParams = true;
                        break;
                }
            }

            if (RoslynHelper.TryParseValue(node.Default?.Value, out Value value))
            {
                p.DefaultValue = value;
                p.DefaultValue.SyntaxNode = node;
            }

            if (RoslynHelper.TryParseType(node.Type, out string type))
            {
                p.Type = type;
            }

            return p;
        }
    }
}