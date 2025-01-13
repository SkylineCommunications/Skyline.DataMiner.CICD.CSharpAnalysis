namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    /// <summary>
    /// Represents a class.
    /// </summary>
    public class ClassClass : CSharpObject<ClassDeclarationSyntax>
    {
        private ClassClass(ClassDeclarationSyntax node) : base(node)
        {
            Access = AccessModifier.None;
            Methods = new List<MethodClass>();
            Properties = new List<PropertyClass>();
            Constructors = new List<ConstructorClass>();
            Fields = new List<FieldClass>();
            InheritanceItems = new List<string>();
            NestedClasses = new List<ClassClass>();
            Attributes = new List<Attribute>();
        }

        /// <summary>
        /// Gets the class namespace.
        /// </summary>
        /// <value>The class namespace.</value>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets the class name.
        /// </summary>
        /// <value>The class name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the class access modifier.
        /// </summary>
        /// <value>The class access modifier.</value>
        public AccessModifier Access { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a static class.
        /// </summary>
        /// <value><c>true</c> if this is a static class;otherwise, <c>false</c>.</value>
        public bool IsStatic { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is an abstract class.
        /// </summary>
        /// <value><c>true</c> if this is an abstract class;otherwise, <c>false</c>.</value>
        public bool IsAbstract { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a sealed class.
        /// </summary>
        /// <value><c>true</c> if this is a sealed class;otherwise, <c>false</c>.</value>
        public bool IsSealed { get; private set; }

        /// <summary>
        /// Gets the methods of the class.
        /// </summary>
        /// <value>The methods of the class.</value>
        public List<MethodClass> Methods { get; }

        /// <summary>
        /// Gets the properties of the class.
        /// </summary>
        /// <value>The properties of the class.</value>
        public List<PropertyClass> Properties { get; }

        /// <summary>
        /// Gets the constructors of the class.
        /// </summary>
        /// <value>The constructors of the class.</value>
        public List<ConstructorClass> Constructors { get; }

        /// <summary>
        /// Gets the fields of the class.
        /// </summary>
        /// <value>The fields of the class.</value>
        public List<FieldClass> Fields { get; }

        /// <summary>
        /// Gets the inherited items of the class.
        /// </summary>
        /// <value>The inherited items of the class.</value>
        public List<string> InheritanceItems { get; }

        /// <summary>
        /// Gets the nested classes of the class.
        /// </summary>
        /// <value>The nested classes of the class.</value>
        public List<ClassClass> NestedClasses { get; }

        /// <summary>
        /// Gets the attributes of the class.
        /// </summary>
        /// <value>The attributes of the class.</value>
        public List<Attribute> Attributes { get; }

        /// <summary>
        /// Gets a value indicating whether this is a partial class.
        /// </summary>
        /// <value><c>true</c> if this is a partial class;otherwise, <c>false</c>.</value>
        public bool IsPartial { get; private set; }

        /// <summary>
        /// Gets the finalizer.
        /// </summary>
        /// <value>The <see cref="FinalizerClass"/> if a finalizer is present. Will be <see langword="null"/> when not present.</value>
        public FinalizerClass Finalizer { get; private set; }

        internal static ClassClass Parse(ClassDeclarationSyntax node)
        {
            ClassClass @class = new ClassClass(node)
            {
                Namespace = String.Empty,
                Name = node.Identifier.Text,
            };
            foreach (var item in node.Modifiers)
            {
                if (RoslynHelper.TryParseAccess(item.Kind(), out AccessModifier access))
                {
                    @class.Access |= access;
                    continue;
                }

                switch (item.Kind())
                {
                    case SyntaxKind.StaticKeyword:
                        @class.IsStatic = true;
                        break;

                    case SyntaxKind.AbstractKeyword:
                        @class.IsAbstract = true;
                        break;

                    case SyntaxKind.SealedKeyword:
                        @class.IsSealed = true;
                        break;

                    case SyntaxKind.PartialKeyword:
                        @class.IsPartial = true;
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
                        @class.Methods.Add(MethodClass.Parse(mds));
                        break;
                    case PropertyDeclarationSyntax pds:
                        @class.Properties.Add(PropertyClass.Parse(pds));
                        break;
                    case FieldDeclarationSyntax fds:
                        @class.Fields.Add(FieldClass.Parse(fds));
                        break;
                    case ClassDeclarationSyntax cds:
                        @class.NestedClasses.Add(ClassClass.Parse(cds));
                        break;
                    case ConstructorDeclarationSyntax cods:
                        @class.Constructors.Add(ConstructorClass.Parse(cods));
                        break;
                    case DestructorDeclarationSyntax dds:
                        @class.Finalizer = FinalizerClass.Parse(dds);
                        break;
                }
            }

            foreach (var item in node.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>())
            {
                if (item is SimpleBaseTypeSyntax sbts && sbts.Type is IdentifierNameSyntax ins)
                {
                    @class.InheritanceItems.Add(ins.Identifier.Text);
                }
            }

            foreach (AttributeListSyntax attributeList in node.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    @class.Attributes.Add(Attribute.Parse(attribute));
                }
            }

            var namespaceDeclaration = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDeclaration != null)
            {
                // Return the namespace name
                @class.Namespace = namespaceDeclaration.Name.ToString();
            }

            return @class;
        }
    }
}