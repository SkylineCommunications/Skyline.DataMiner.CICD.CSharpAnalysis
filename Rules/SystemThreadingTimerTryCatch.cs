using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Skyline.DataMiner.CICD.CSharpAnalysis.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SystemThreadingTimerTryCatch : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TIMER001";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "Timer callback must contain try/catch",
                messageFormat: "Timer callback should contain a try/catch block to prevent unobserved exceptions",
                category: "Reliability",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "System.Threading.Timer callbacks must handle exceptions to avoid process crashes.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // Don't analyze generated code
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var creation = (ObjectCreationExpressionSyntax)context.Node;

            // Must be System.Threading.Timer
            var typeSymbol = context.SemanticModel.GetSymbolInfo(creation.Type).Symbol as INamedTypeSymbol;
            if (typeSymbol == null)
                return;

            if (typeSymbol.ToString() != "System.Threading.Timer")
                return;

            // Must have at least one argument: TimerCallback callback
            if (creation.ArgumentList == null || creation.ArgumentList.Arguments.Count == 0)
                return;

            var callbackArg = creation.ArgumentList.Arguments[0].Expression;

            switch (callbackArg)
            {
                // Lambda callback
                case SimpleLambdaExpressionSyntax lambda:
                    CheckLambda(context, lambda.Block);
                    break;

                case ParenthesizedLambdaExpressionSyntax lambda:
                    CheckLambda(context, lambda.Block);
                    break;

                // Identifier (method reference)
                case IdentifierNameSyntax identifier:
                    CheckMethodCallback(context, identifier);
                    break;
            }
        }

        private static void CheckLambda(SyntaxNodeAnalysisContext context, BlockSyntax block)
        {
            // No block → cannot analyze
            if (block == null)
                return;

            var hasTry = block.Statements.OfType<TryStatementSyntax>().Any();

            if (!hasTry)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, block.GetLocation()));
            }
        }

        private static void CheckMethodCallback(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifier)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(identifier).Symbol as IMethodSymbol;

            // Find method declaration
            var declSyntax = symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
            if (declSyntax?.Body == null)
                return;

            var hasTry = declSyntax.Body.Statements.OfType<TryStatementSyntax>().Any();

            if (!hasTry)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, identifier.GetLocation()));
            }
        }
    }
}