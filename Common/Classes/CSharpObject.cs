namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System;

    using Microsoft.CodeAnalysis;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Interfaces;

    /// <summary>
    /// Represents a C# object.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public class CSharpObject<T> : ICSharpObject<T> where T : SyntaxNode
    {
        /// <summary>
        /// With this constructor you need to set the SyntaxNode manually.
        /// </summary>
        internal CSharpObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpObject{T}"/> class.
        /// </summary>
        /// <param name="syntaxNode">The syntax node.</param>
        /// <exception cref="ArgumentNullException"><paramref name="syntaxNode"/> is <see langword="null"/>.</exception>
        protected CSharpObject(T syntaxNode)
        {
            SyntaxNode = syntaxNode ?? throw new ArgumentNullException(nameof(syntaxNode));
        }

        /// <summary>
        /// Gets the syntax node.
        /// </summary>
        /// <value>The syntax node.</value>
        public T SyntaxNode { get; internal set; }

        /// <summary>
        /// Retrieves the location.
        /// </summary>
        /// <returns>The location.</returns>
        public Location GetLocation()
        {
            return SyntaxNode?.GetLocation();
        }
    }
}