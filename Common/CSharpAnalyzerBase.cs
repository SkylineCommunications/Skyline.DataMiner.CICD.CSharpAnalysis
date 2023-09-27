namespace Skyline.DataMiner.CICD.CSharpAnalysis
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

    /// <summary>
    /// C# analyzer base class.
    /// </summary>
    public abstract class CSharpAnalyzerBase
    {
        /// <summary>
        /// Tries canceling the operation.
        /// </summary>
        protected void Cancel()
        {
            IsCanceled = true;
        }

        /// <summary>
        /// Gets a value indicating whether the operation has been canceled.
        /// </summary>
        /// <value><c>true</c> if canceled; otherwise, <c>false</c>.</value>
        public bool IsCanceled { get; private set; }

        /// <summary>
        /// Checks the specified assignment expression.
        /// </summary>
        /// <param name="assignment">The assignment expression.</param>
        public virtual void CheckAssignmentExpression(AssignmentExpressionClass assignment) { }

        /// <summary>
        /// Checks the specified object creation.
        /// </summary>
        /// <param name="objectCreation">The object creation.</param>
        public virtual void CheckObjectCreation(ObjectCreationClass objectCreation) { }

        /// <summary>
        /// Checks the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        public virtual void CheckMethod(MethodClass method) { }

        /// <summary>
        /// Checks the specified class.
        /// </summary>
        /// <param name="classClass ">The class.</param>
        public virtual void CheckClass(ClassClass classClass) { }

        /// <summary>
        /// Checks the specified calling method.
        /// </summary>
        /// <param name="callingMethod  ">The calling method.</param>
        public virtual void CheckCallingMethod(CallingMethodClass callingMethod) { }

        /// <summary>
        /// Checks the specified interface.
        /// </summary>
        /// <param name="interface">The interface.</param>
        public virtual void CheckInterface(InterfaceClass @interface) { }

        /// <summary>
        /// Checks the #define preprocessor directive.
        /// </summary>
        /// <param name="directiveName">Name of the directive.</param>
        /// <param name="directive">Roslyn object of the directive.</param>
        public virtual void CheckDefineDirective(string directiveName, DefineDirectiveTriviaSyntax directive) { }

        /// <summary>
        /// Checks the #if preprocessor directive.
        /// </summary>
        /// <param name="directiveName">Name of the directive.</param>
        /// <param name="directive">Roslyn object of the directive.</param>
        public virtual void CheckIfDirective(string directiveName, IfDirectiveTriviaSyntax directive) { }

        /// <summary>
        /// Checks the specified comment line.
        /// </summary>
        /// <param name="commentLine">The comment line.</param>
        /// <param name="trivia">The trivia.</param>
        public virtual void CheckCommentLine(string commentLine, SyntaxTrivia trivia) { }

        /// <summary>
        /// Checks the specified constructor.
        /// </summary>
        /// <param name="method">The constructor.</param>
        public virtual void CheckConstructor(ConstructorClass constructor) { }
    }
}