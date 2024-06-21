namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents a finalizer (formerly known as deconstructor).
    /// </summary>
    public class FinalizerClass : CSharpObject<DestructorDeclarationSyntax>
    {
        private FinalizerClass(DestructorDeclarationSyntax node) : base(node)
        {
        }
        
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }
        
        internal static FinalizerClass Parse(DestructorDeclarationSyntax node)
        {
            var ctor = new FinalizerClass(node)
            {
                Name = node.Identifier.Text
            };

            return ctor;
        }
    }
}