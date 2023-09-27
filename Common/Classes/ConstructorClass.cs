namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
	using System.Collections.Generic;

	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

	/// <summary>
	/// Represents a constructor.
	/// </summary>
	public class ConstructorClass : CSharpObject<ConstructorDeclarationSyntax>
    {
        private ConstructorClass(ConstructorDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
            Parameters = new List<ParameterClass>();
        }

		/// <summary>
		/// Gets the access modifier.
		/// </summary>
		/// <value>The access modifier.</value>
		public AccessModifier Access { get; private set; }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is a static class.
		/// </summary>
		/// <value><c>true</c> if this is a static class;otherwise, <c>false</c>.</value>
		public bool IsStatic { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public List<ParameterClass> Parameters { get; }


        internal static ConstructorClass Parse(ConstructorDeclarationSyntax node)
        {
            var ctor = new ConstructorClass(node)
            {
                Name = node.Identifier.Text
            };

            foreach (var item in node.Modifiers)
            {
                if (RoslynHelper.TryParseAccess(item.Kind(), out AccessModifier access))
                {
                    ctor.Access |= access;
                    continue;
                }

                switch (item.Kind())
                {
                    case SyntaxKind.StaticKeyword:
                        ctor.IsStatic = true;
                        break;

                    default:
                        // Unknown modifier
                        break;
                }
            }

            foreach (var item in node.ParameterList?.Parameters ?? new SeparatedSyntaxList<ParameterSyntax>())
            {
                ctor.Parameters.Add(ParameterClass.Parse(item));
            }

            // TODO: ? Also indicate base() or this()?

            return ctor;
        }
    }
}