using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Skyline.DataMiner.CICD.CSharpAnalysis.Analyzer
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
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(
                AnalyzeObjectCreation,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression);
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is BaseObjectCreationExpressionSyntax creation))
                return;

            // Only analyze System.Threading.Timer
            if (context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type?.ToString() != "System.Threading.Timer")
                return;

            var args = creation.ArgumentList?.Arguments;
            if (args == null || args.Value.Count == 0)
                return;

            // Map arguments to parameters
            var ctor = context.SemanticModel.GetSymbolInfo(creation, context.CancellationToken).Symbol as IMethodSymbol;
            if (ctor != null && ctor.MethodKind != MethodKind.Constructor)
                return;

            // Resolve the callback parameter
            var callbackParam = ctor.Parameters
                .FirstOrDefault(p => p.Name == "callback" ||
                                     p.Type.ToString() == "System.Threading.TimerCallback");
            if (callbackParam == null)
                return;

            // Find the argument bound to the callback parameter
            var callbackArgument = args.Value.FirstOrDefault(arg =>
            {
                if (arg.NameColon != null)
                    return arg.NameColon.Name.Identifier.ValueText == callbackParam.Name;

                var index = args.Value.IndexOf(arg);
                return index == ctor.Parameters.IndexOf(callbackParam);
            });

            if (callbackArgument == null)
                return;

            AnalyzeCallbackExpression(context, callbackArgument.Expression);
        }

        private static void AnalyzeCallbackExpression(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax expr)
        {
            switch (expr)
            {
                case LambdaExpressionSyntax lambda:
                    CheckLambda(context, lambda);
                    break;

                case AnonymousMethodExpressionSyntax anon:
                    CheckBlockForTry(context, anon.Block, expr.GetLocation());
                    break;

                case IdentifierNameSyntax _:
                case MemberAccessExpressionSyntax _:
                    CheckMethodCallback(context, expr);
                    break;

                    // All other expressions are irrelevant
            }
        }

        private static void CheckLambda(SyntaxNodeAnalysisContext context, LambdaExpressionSyntax lambda)
        {
            switch (lambda.Body)
            {
                case BlockSyntax block:
                    CheckBlockForTry(context, block, lambda.GetLocation());
                    break;

                case ExpressionSyntax _:
                    // Expression-bodied lambdas cannot contain try/catch
                    context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
                    break;
            }
        }

        private static void CheckMethodCallback(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax methodGroup)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(methodGroup, context.CancellationToken).Symbol as IMethodSymbol;

            var decl = symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;
            if (decl?.Body == null)
                return;

            CheckBlockForTry(context, decl.Body, methodGroup.GetLocation());
        }

        private static void CheckBlockForTry(
            SyntaxNodeAnalysisContext context,
            BlockSyntax block,
            Location reportLocation)
        {
            if (block == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, reportLocation));
                return;
            }

            var hasTry = block.DescendantNodes().OfType<TryStatementSyntax>().Any();

            if (!hasTry)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, reportLocation));
            }
        }
    }
}