namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    /// <summary>
    /// Represents a field.
    /// </summary>
    public class FieldClass : CSharpObject<FieldDeclarationSyntax>
    {
        private FieldClass(FieldDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
            Attributes = new List<Attribute>();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the access modifier.
        /// </summary>
        /// <value>The access modifier.</value>
        public AccessModifier Access { get; internal set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; internal set; }

        //public bool HasValueAssigned { get; internal set; }

        //public object AssignedValue { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this is a read-ony field.
        /// </summary>
        /// <value><c>true</c> if this is a read-only field;otherwise, <c>false</c>.</value>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a static field.
        /// </summary>
        /// <value><c>true</c> if this is a static field;otherwise, <c>false</c>.</value>
        public bool IsStatic { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this field has the new keyword.
        /// </summary>
        /// <value><c>true</c> if this field has the new keyword;otherwise, <c>false</c>.</value>
        public bool IsNew { get; private set; }

        /// <summary>
        /// Gets the attributes of the field.
        /// </summary>
        /// <value>The attributes of the field.</value>
        public List<Attribute> Attributes { get; }

        /// <summary>
        /// Parses the field declaration syntax node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>An instance of <see cref="FieldClass"/>.</returns>
        public static FieldClass Parse(FieldDeclarationSyntax node)
        {
            var field = new FieldClass(node);

            foreach (var item in node.Modifiers)
            {
                if (RoslynHelper.TryParseAccess(item.Kind(), out AccessModifier access))
                {
                    field.Access |= access;
                    continue;
                }

                switch (item.Kind())
                {
                    case SyntaxKind.ReadOnlyKeyword:
                        field.IsReadOnly = true;
                        break;

                    case SyntaxKind.StaticKeyword:
                        field.IsStatic = true;
                        break;

                    case SyntaxKind.NewKeyword:
                        field.IsNew = true;
                        break;
                }
            }

            if (node.Declaration is VariableDeclarationSyntax vds)
            {
                if (RoslynHelper.TryParseType(vds.Type, out string type))
                {
                    field.Type = type;
                }

                for (int i = 0; i < vds.Variables.Count; i++)
                {
                    var variable = vds.Variables[i];
                    if (i == 0)
                    {
                        field.Name = variable.Identifier.Text;
                    }

                    // EqualsValueClause
                    // TODO: retrieve the value
                }
            }

            foreach (AttributeListSyntax attributeList in node.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    field.Attributes.Add(Attribute.Parse(attribute));
                }
            }

            return field;
        }
    }
}