namespace Skyline.DataMiner.CICD.CSharpAnalysis.Interfaces
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a C# object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public interface ICSharpObject<out T> where T : SyntaxNode
    {
        /// <summary>
        /// Gets the syntax node.
        /// </summary>
        /// <value>The syntax node.</value>
        T SyntaxNode { get; }

        /// <summary>
        /// Retrieves the location.
        /// </summary>
        /// <returns>The location.</returns>
        Location GetLocation();
    }
}