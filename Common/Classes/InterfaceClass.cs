namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    /// <summary>
    /// Represents an interface.
    /// </summary>
    public class InterfaceClass : CSharpObject<InterfaceDeclarationSyntax>
    {
        private InterfaceClass(InterfaceDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
            Methods = new List<MethodClass>();
            Properties = new List<PropertyClass>();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the access modifier.
        /// </summary>
        /// <value>The access modifier.</value>
        public AccessModifier Access { get; private set; }

        /// <summary>
        /// Gets the methods of the interface.
        /// </summary>
        /// <value>The methods of the interface.</value>
        public List<MethodClass> Methods { get; }

        /// <summary>
        /// Gets the properties of the interface.
        /// </summary>
        /// <value>The properties of the interface.</value>
        public List<PropertyClass> Properties { get; }

        //public List<IndexerClass> Indexers { get; }

        //public List<object> Events { get; }

        /// <summary>
        /// Gets a value indicating whether this is a partial interface.
        /// </summary>
        /// <value><c>true</c> if this is a partial interface;otherwise, <c>false</c>.</value>
        public bool IsPartial { get; private set; }

        internal static InterfaceClass Parse(InterfaceDeclarationSyntax node)
        {
            var @interface = new InterfaceClass(node)
            {
                Name = node.Identifier.Text,
            };
            foreach (var item in node.Modifiers)
            {
                if (RoslynHelper.TryParseAccess(item.Kind(), out AccessModifier access))
                {
                    @interface.Access |= access;
                    continue;
                }

                switch (item.Kind())
                {
                    case SyntaxKind.PartialKeyword:
                        @interface.IsPartial = true;
                        break;

                    default:
                        // Unknown modifier
                        break;
                }
            }

            foreach (var item in node.ChildNodes())
            {
                switch (item)
                {
                    case MethodDeclarationSyntax mds:
                        @interface.Methods.Add(MethodClass.Parse(mds));
                        break;
                    case PropertyDeclarationSyntax pds:
                        @interface.Properties.Add(PropertyClass.Parse(pds));
                        break;
                    case IndexerDeclarationSyntax ids:
                        // @interface.Indexers.Add(IndexerClass.Parse(ids));
                        break;
                    case EventDeclarationSyntax eds:
                        // @interface.Events.Add(EventClass.Parse(eds));
                        break;
                }
            }

            return @interface;
        }
    }
}