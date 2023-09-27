namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Represents an object creation expression.
    /// </summary>
    public class ObjectCreationClass : CSharpObject<ObjectCreationExpressionSyntax>
    {
        private ObjectCreationClass(ObjectCreationExpressionSyntax node) : base(node)
        {
            Arguments = new List<Argument>();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public List<Argument> Arguments { get; }

        internal static ObjectCreationClass Parse(ObjectCreationExpressionSyntax node)
        {
            ObjectCreationClass objectCreation = new ObjectCreationClass(node);

            if (RoslynHelper.TryParseType(node.Type, out string name))
            {
                objectCreation.Name = name;
            }

            // Initializer is probably property constructor

            foreach (var arg in node.ArgumentList?.Arguments ?? new SeparatedSyntaxList<ArgumentSyntax>())
            {
                objectCreation.Arguments.Add(new Argument(arg));
            }

            return objectCreation;
        }
    }
}