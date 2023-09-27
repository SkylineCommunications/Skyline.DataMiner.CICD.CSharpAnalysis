namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

	/// <summary>
	/// Represents a property.
	/// </summary>
	public class PropertyClass : CSharpObject<PropertyDeclarationSyntax>
    {
        internal PropertyClass(PropertyDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; internal set; }

		/// <summary>
		/// Gets the class access modifier.
		/// </summary>
		/// <value>The class access modifier.</value>
		public AccessModifier Access { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this is a getter.
		/// </summary>
		/// <value><c>true</c> if this is a getter; otherwise, <c>false</c>.</value>
		public bool IsGetter { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this is a setter.
		/// </summary>
		/// <value><c>true</c> if this is a setter; otherwise, <c>false</c>.</value>
		public bool IsSetter { get; internal set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this is a virtual property.
		/// </summary>
		/// <value><c>true</c> if this is a virtual property; otherwise, <c>false</c>.</value>
		public bool IsVirtual { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this is an override.
		/// </summary>
		/// <value><c>true</c> if this is an override; otherwise, <c>false</c>.</value>
		public bool IsOverride { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this property has the new keyword.
		/// </summary>
		/// <value><c>true</c> if this property has the new keyword; otherwise, <c>false</c>.</value>
		public bool IsNew { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is a static property.
		/// </summary>
		/// <value><c>true</c> if this is a static property; otherwise, <c>false</c>.</value>
		public bool IsStatic { get; internal set; }

        public static PropertyClass Parse(PropertyDeclarationSyntax node)
        {
            var prop = new PropertyClass(node)
            {
                Name = node.Identifier.Text
            };

            foreach (var item in node.Modifiers)
            {
                if (RoslynHelper.TryParseAccess(item.Kind(), out AccessModifier access))
                {
                    prop.Access |= access;
                    continue;
                }

                switch (item.Kind())
                {
                    case SyntaxKind.VirtualKeyword:
                        prop.IsVirtual = true;
                        break;

                    case SyntaxKind.OverrideKeyword:
                        prop.IsOverride = true;
                        break;

                    case SyntaxKind.StaticKeyword:
                        prop.IsStatic = true;
                        break;

                    case SyntaxKind.NewKeyword:
                        prop.IsNew = true;
                        break;
                }
            }

            if (RoslynHelper.TryParseType(node.Type, out string type))
            {
                prop.Type = type;
            }

            foreach (var item in node.AccessorList?.Accessors ?? new SyntaxList<AccessorDeclarationSyntax>())
            {
                switch (item.Kind())
                {
                    case SyntaxKind.GetAccessorDeclaration:
                        prop.IsGetter = true;
                        break;
                    case SyntaxKind.SetAccessorDeclaration:
                        prop.IsSetter = true;
                        break;

                    // TODO: AccessModifier of the accessor isn't parsed yet
                }
            }

            // TODO: Check for assigned values?
            /*
             * public string Property3 => "ABC";
             * public virtual string Property6 { get; } = "ABC";
             */

            return prop;
        }
    }
}