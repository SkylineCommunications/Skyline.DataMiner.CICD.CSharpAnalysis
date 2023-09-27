namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents an attribute.
    /// </summary>
    public class Attribute : CSharpObject<AttributeSyntax>
    {
        internal Attribute(AttributeSyntax node) : base(node)
        {
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        internal static Attribute Parse(AttributeSyntax node)
        {
            Attribute attr = new Attribute(node)
            {
                Name = node.Name.ToString(),
            };

            // TODO: Arguments
            // TODO: Target?

            return attr;
        }
    }
}