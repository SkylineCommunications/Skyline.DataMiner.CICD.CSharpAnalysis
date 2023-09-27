namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    /// <summary>
    /// Represents a method.
    /// </summary>
    public class MethodClass : CSharpObject<MethodDeclarationSyntax>
    {
        private MethodClass(MethodDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
            Parameters = new List<ParameterClass>();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

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
		/// Gets a value indicating whether this is an override.
		/// </summary>
		/// <value><c>true</c> if this is an override;otherwise, <c>false</c>.</value>
		public bool IsOverride { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is a virtual method.
		/// </summary>
		/// <value><c>true</c> if this is a virtual method;otherwise, <c>false</c>.</value>
		public bool IsVirtual { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is an abstract method.
		/// </summary>
		/// <value><c>true</c> if this is an abstract method;otherwise, <c>false</c>.</value>
		public bool IsAbstract { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this method uses the new keyword.
		/// </summary>
		/// <value><c>true</c> if this method uses the new keyword, <c>false</c>.</value>
		public bool IsNew { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public List<ParameterClass> Parameters { get; }

		/// <summary>
		/// Gets a value indicating whether this is a sealed method.
		/// </summary>
		/// <value><c>true</c> if this is a sealed method;otherwise, <c>false</c>.</value>
		public bool IsSealed { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this is a static method.
		/// </summary>
		/// <value><c>true</c> if this is a static method;otherwise, <c>false</c>.</value>
		public bool IsStatic { get; private set; }

        internal static MethodClass Parse(MethodDeclarationSyntax node)
        {
            MethodClass method = new MethodClass(node)
            {
                Name = node.Identifier.Text,
                ReturnType = node.ReturnType.ToString(),
            };
            foreach (SyntaxToken modifier in node.Modifiers)
            {
                if (RoslynHelper.TryParseAccess(modifier.Kind(), out AccessModifier access))
                {
                    method.Access |= access;
                    continue;
                }

                switch (modifier.Kind())
                {
                    case SyntaxKind.StaticKeyword:
                        method.IsStatic = true;
                        break;

                    case SyntaxKind.AbstractKeyword:
                        method.IsAbstract = true;
                        break;

                    case SyntaxKind.SealedKeyword:
                        method.IsSealed = true;
                        break;

                    case SyntaxKind.OverrideKeyword:
                        method.IsOverride = true;
                        break;

                    case SyntaxKind.NewKeyword:
                        method.IsNew = true;
                        break;

                    case SyntaxKind.VirtualKeyword:
                        method.IsVirtual = true;
                        break;
                }
            }

            foreach (ParameterSyntax parameter in node.ParameterList?.Parameters ?? new SeparatedSyntaxList<ParameterSyntax>())
            {
                method.Parameters.Add(ParameterClass.Parse(parameter));
            }

            return method;
        }
    }
}