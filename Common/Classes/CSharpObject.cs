namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
	using System;

	using Interfaces;

	using Microsoft.CodeAnalysis;

	/// <summary>
	/// Represents a C# object.
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	public class CSharpObject<T> : ICSharpObject<T> where T: SyntaxNode
    {
        /// <summary>
        /// With this constructor you need to set the SyntaxNode manually.
        /// </summary>
        internal CSharpObject()
        {
        }

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