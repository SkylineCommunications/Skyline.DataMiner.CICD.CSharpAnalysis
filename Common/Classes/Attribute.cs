namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents an attribute.
    /// </summary>
    public class Attribute : CSharpObject<AttributeSyntax>
    {
        private Attribute(AttributeSyntax node) : base(node)
        {
            Arguments = new List<AttributeArgument>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public List<AttributeArgument> Arguments { get; }
        
        internal static Attribute Parse(AttributeSyntax node)
        {
            Attribute attr = new Attribute(node)
            {
                Name = node.Name.ToString(),
            };

            foreach (var arg in node.ArgumentList?.Arguments ?? new SeparatedSyntaxList<AttributeArgumentSyntax>())
            {
                attr.Arguments.Add(new AttributeArgument(arg));
            }

            return attr;
        }
    }
}