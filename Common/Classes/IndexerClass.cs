namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    /// <summary>
    /// Represents an indexer.
    /// </summary>
    public class IndexerClass : CSharpObject<IndexerDeclarationSyntax>
    {
        internal IndexerClass(IndexerDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
            Parameters = new List<Parameter>();
        }

        /// <summary>
        /// Gets the return type.
        /// </summary>
        /// <value>The return type.</value>
        public string ReturnType { get; private set; }

        /// <summary>
        /// Gets the access modifier.
        /// </summary>
        /// <value>The access modifier.</value>
        public AccessModifier Access { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a getter.
        /// </summary>
        /// <value><c>true</c> if this is a getter;otherwise, <c>false</c>.</value>
        public bool IsGetter { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a setter.
        /// </summary>
        /// <value><c>true</c> if this is a setter;otherwise, <c>false</c>.</value>
        public bool IsSetter { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public List<Parameter> Parameters { get; }

        /// <summary>
        /// Represents an indexer parameter.
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// Gets the parameter type.
            /// </summary>
            /// <value>The parameter type.</value>
            public string Type { get; internal set; }

            /// <summary>
            /// Gets the parameter name.
            /// </summary>
            /// <value>The parameter name.</value>
            public string Name { get; internal set; }
        }
    }
}