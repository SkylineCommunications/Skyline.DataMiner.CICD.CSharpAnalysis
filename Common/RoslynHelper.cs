namespace Skyline.DataMiner.CICD.CSharpAnalysis
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FindSymbols;
    using Microsoft.CodeAnalysis.Scripting;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;
    using Skyline.DataMiner.CICD.CSharpAnalysis.Enums;

    /// <summary>
    /// Roslyn helper class.
    /// </summary>
	public static class RoslynHelper
    {
        /// <summary>
        /// Retrieves the symbol of the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns>The symbol.</returns>
        public static ISymbol GetSymbol(SyntaxNode expression, SemanticModel semanticModel)
        {
            return semanticModel.GetSymbolInfo(expression).Symbol ?? semanticModel.GetDeclaredSymbol(expression);
        }

        /// <summary>
        /// Tries parsing the specified type syntax.
        /// </summary>
        /// <param name="typeSyntax">The type syntax.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryParseType(TypeSyntax typeSyntax, out string type)
        {
            switch (typeSyntax)
            {
                case PredefinedTypeSyntax pts:
                    type = pts.Keyword.ValueText;
                    return true;
                case GenericNameSyntax gns:
                    type = gns.ToString();
                    return true;
                case ArrayTypeSyntax ats:
                    type = ats.ToString();
                    return true;
                case IdentifierNameSyntax ins:
                    type = ins.Identifier.Text;
                    return true;
                default:
                    type = null;
                    return false;
            }
        }

        /// <summary>
        /// Tries parsing the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="access">The access modifier.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryParseAccess(SyntaxKind value, out AccessModifier access)
        {
            switch (value)
            {
                case SyntaxKind.PublicKeyword:
                    access = AccessModifier.Public;
                    return true;

                case SyntaxKind.PrivateKeyword:
                    access = AccessModifier.Private;
                    return true;

                case SyntaxKind.ProtectedKeyword:
                    access = AccessModifier.Protected;
                    return true;

                case SyntaxKind.InternalKeyword:
                    access = AccessModifier.Internal;
                    return true;
            }

            access = AccessModifier.None;
            return false;
        }

        private static ConcurrentDictionary<string, object> _tryResolveExpressionCache = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Tries to resolve the expression. Similar like the C# Interactive from VS.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="hasNotChanged">Dedicates what the resolved value will have for <see cref="Value.HasNotChanged"/>.</param>
        /// <param name="value">The resulting value.</param>
        /// <returns><c>true</c> if the expression could be evaluated to a value; otherwise, <c>false</c>.</returns>
        public static bool TryResolveExpression(ExpressionSyntax expression, bool hasNotChanged, out Value value)
        {
            if (expression == null)
            {
                value = null;
                return false;
            }

            var result = _tryResolveExpressionCache.GetOrAdd(expression.ToString(),
                e =>
                {
                    try
                    {
                        var options = ScriptOptions.Default.WithImports("System");
                        // TODO: See if it's possible to check which are being used in the code and add them to the options?

                        return Task.Run(async () => await CSharpScript.EvaluateAsync(e, options)).Result;
                    }
                    catch (AggregateException)
                    {
                        // Failed to resolve the expression
                        return null;
                    }
                });

            if (result != null)
            {
                value = new Value
                {
                    Object = result,
                    Type = ValueTypeConverter.GetValueType(result.GetType()),
                    HasNotChanged = hasNotChanged
                };
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="namespace">The namespace.</param>
        /// <returns><c>true</c> if the symbol is of the specified type; otherwise, <c>false</c>.</returns>
        public static bool CheckIfCertainClass(ISymbol symbol, SemanticModel semanticModel, Solution solution, string assemblyName, string @namespace)
        {
            if (symbol == null || @namespace == null)
            {
                return false;
            }

            if (symbol.ContainingAssembly?.Name == assemblyName)
            {
                return symbol.ToString().StartsWith(@namespace);
            }

            var refsConst = symbol.DeclaringSyntaxReferences;
            if (refsConst.IsEmpty)
            {
                return false;
            }

            var syntaxConst = refsConst[0].GetSyntax();
            if (!(syntaxConst is VariableDeclaratorSyntax vdsConst) || vdsConst.Initializer?.Value == null)
            {
                return false;
            }

            try
            {
                var paramSymbolInfo = semanticModel.GetSymbolInfo(vdsConst.Initializer.Value);
                var paramSymbol = paramSymbolInfo.Symbol;

                return CheckIfCertainClass(paramSymbol, semanticModel, solution, assemblyName, @namespace);
            }
            catch (ArgumentException)
            {
                /* Couldn't find matching symbol */

                // Different semantic model is needed
                var locations = symbol.Locations;

                foreach (Project solutionProject in solution.Projects)
                {
                    var compilation = solutionProject.GetCompilationAsync().Result;

                    if (!compilation.Assembly.Equals(symbol.ContainingAssembly))
                    {
                        continue;
                    }

                    foreach (Location location in locations)
                    {
                        var newSemanticModel = compilation.GetSemanticModel(location.SourceTree);

                        // Check if the symbol can be resolved in the other syntax trees (different cs files)
                        if (CheckIfCertainClass(symbol, newSemanticModel, solution, assemblyName, @namespace))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries retrieving the variable value.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value could be retrieved; otherwise, <c>false</c>.</returns>
        public static bool TryGetVariableValue(ExpressionSyntax expression, SemanticModel semanticModel, Solution solution, out Value value)
        {
            value = null;
            ISymbol symbol;
            try
            {
                symbol = semanticModel.GetSymbolInfo(expression).Symbol;
            }
            catch (ArgumentException e)
                when (String.Equals(e.Message, "Syntax node is not within syntax tree", StringComparison.OrdinalIgnoreCase))
            {
                // In case variable is defined in another QAction (Chained back to precompile (different semantic model)
                // TODO: Maybe an idea to see if we can 'preload' the precompile and also supply that with the TryParseValue?
                // In case of these exceptions, then we can search it in the precompile. 
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            if (symbol == null)
            {
                // Couldn't find the symbol in this semantic model (item from another QAction?...)
                return false;
            }

            if (symbol is IFieldSymbol fs && fs.HasConstantValue)
            {
                value = new Value
                {
                    _Symbol = fs,
                    Object = fs.ConstantValue,
                    Type = ValueTypeConverter.GetValueType(fs.ConstantValue?.GetType()),
                    HasNotChanged = true
                };
                return true;
            }

            if (symbol is ILocalSymbol ls && ls.HasConstantValue)
            {
                value = new Value
                {
                    _Symbol = ls,
                    Object = ls.ConstantValue,
                    Type = ValueTypeConverter.GetValueType(ls.ConstantValue?.GetType()),
                    HasNotChanged = true
                };
                return true;
            }

            var refs = symbol.DeclaringSyntaxReferences;
            if (refs.IsEmpty)
            {
                return false;
            }

            var syntax = refs[0].GetSyntax();

            if (syntax is ParameterSyntax ps)
            {
                Value.ValueType type = Value.ValueType.Unknown;
                Value.ValueType arrayType = Value.ValueType.Unknown;
                if (ps.Type is PredefinedTypeSyntax pts)
                {
                    type = ValueTypeConverter.GetValueType(pts.Keyword.Kind());
                }
                else if (ps.Type is ArrayTypeSyntax ats)
                {
                    type = Value.ValueType.Array;

                    if (ats.ElementType is PredefinedTypeSyntax ptsArray)
                    {
                        arrayType = ValueTypeConverter.GetValueType(ptsArray.Keyword.Kind());
                    }
                }

                // Argument of a method
                value = new Value
                {
                    Type = type,
                    ArrayType = arrayType,
                    IsMethodArgument = true
                };
                return true;
            }

            if (!(syntax is VariableDeclaratorSyntax vds))
            {
                return false;
            }

            bool isReadOnly = vds.Parent.Parent is FieldDeclarationSyntax fds && fds.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);

            List<ExpressionSyntax> otherExpressionsThatImpact = new List<ExpressionSyntax>();
            // Will be set to false as soon as we detect a change.
            bool isConst = true;

            // Check if variable has been changed or passed as a ref.
            var references = SymbolFinder.FindReferencesAsync(symbol, solution).Result;
            ReferenceLocation[] locations = references.SelectMany(referencedSymbol => referencedSymbol.Locations).ToArray();
            foreach (ReferenceLocation location in locations)
            {
                // Check for each location if the value is changed (isWrittenTo)
                // isWrittenTo is a non-public member, so access the value via the way underneath.
                PropertyInfo isWrittenToProperty = location.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Single(pi => pi.Name == "IsWrittenTo");
                bool isWrittenTo = (bool)isWrittenToProperty.GetValue(location, null);
                if (!isWrittenTo)
                {
                    continue;
                }

                SyntaxNode parentNode = location.Location.SourceTree.GetRoot().FindNode(location.Location.SourceSpan)?.Parent;
                if (parentNode is ExpressionSyntax es)
                {
                    otherExpressionsThatImpact.Add(es);
                }

                isConst = false;
            }

            if (!locations.Any())
            {
                // Currently to be sure that if somehow we can't find any references or locations that it won't count as a constant.
                // Better to have an uncertain than a false certain.
                isConst = false;
            }

            ExpressionSyntax initializerExpressionSyntax = vds.Initializer?.Value;
            if (initializerExpressionSyntax == null)
            {
                if (otherExpressionsThatImpact.Count == 1 && otherExpressionsThatImpact[0] is AssignmentExpressionSyntax aes && aes.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    initializerExpressionSyntax = aes.Right;
                    isConst = true; // Only 1 assignment was done, so value will not have changed

                    if (isReadOnly)
                    {
                        // We can't be sure when it is set in the constructor. Maybe it's in an if.
                        isConst = false;
                    }
                }
                else
                {
                    // Not a simple assignment OR multiple assignments

                    // Currently we don't support this as it's very unclear when something is filled in. Basically we can't guarantee anything...
                    return false;
                }
            }

            if (TryParseValue(initializerExpressionSyntax, semanticModel, solution, out Value v))
            {
                value = v;
                value.HasNotChanged = isConst && v.HasNotChanged; // In case the underlying initializer isn't constant
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries retrieving the variable assignment.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="solution">The solution</param>
        /// <param name="assignmentExpression">The assignment expression.</param>
        /// <returns><c>true</c> if the expression could be retrieved; otherwise, <c>false</c>.</returns>
        public static bool TryGetVariableAssignment(AssignmentExpressionSyntax expression, SemanticModel semanticModel, Solution solution, out ExpressionSyntax assignmentExpression)
        {
            assignmentExpression = null;
            ISymbol symbol;
            try
            {
                if (expression.Left is MemberAccessExpressionSyntax maes)
                {
                    symbol = semanticModel.GetSymbolInfo(maes.Expression).Symbol;
                }
                else
                {
                    symbol = semanticModel.GetSymbolInfo(expression.Left).Symbol;
                }
            }
            catch (ArgumentException e)
                when (String.Equals(e.Message, "Syntax node is not within syntax tree", StringComparison.OrdinalIgnoreCase))
            {
                // In case variable is defined in another QAction (Chained back to precompile (different semantic model)
                // TODO: Maybe an idea to see if we can 'preload' the precompile and also supply that with the TryParseValue?
                // In case of these exceptions, then we can search it in the precompile. 
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            if (symbol == null)
            {
                // Couldn't find the symbol in this semantic model (item from another QAction?...)
                return false;
            }

            if (symbol is IFieldSymbol fs)
            {
                // Not implemented yet
            }

            if (symbol is ILocalSymbol ls)
            {
                if (ls.DeclaringSyntaxReferences.Length != 1)
                {
                    // Multiple declarations (e.g.: partial classes)
                    // Won't support for now.
                    return false;
                }

                var reference = ls.DeclaringSyntaxReferences.First();
                if (reference.SyntaxTree.GetRoot().FindNode(reference.Span) is VariableDeclaratorSyntax vds)
                {
                    assignmentExpression = vds.Initializer?.Value;
                    return assignmentExpression != null;
                }
            }

            if (symbol is IPropertySymbol ps)
            {
                // Not implemented yet
            }

            return false;
        }

        /// <summary>
        /// Tries parsing the specified expression as a value.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Lite version of the TryParseValue that doesn't need the semantic model and solution. Currently this will only work for LiteralExpressionSyntax.
        /// </remarks>
        public static bool TryParseValue(ExpressionSyntax expression, out Value value)
        {
            bool succeeded = false;
            value = null;

            switch (expression)
            {
                case LiteralExpressionSyntax les:
                    {
                        value = new Value
                        {
                            Object = les.Token.Value,
                            Type = ValueTypeConverter.GetValueType(les.Token),
                            HasNotChanged = true,
                        };

                        succeeded = true;
                        break;
                    }
            }

            return succeeded;
        }

        /// <summary>
        /// Tries parsing the specified expression as a value.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the parse succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool TryParseValue(ExpressionSyntax expression, SemanticModel semanticModel, Solution solution, out Value value)
        {
            bool succeeded = false;
            value = null;

            if (TryParseValue(expression, out Value liteValue))
            {
                value = liteValue;
                return true;
            }

            switch (expression)
            {
                case PrefixUnaryExpressionSyntax pues:
                    {
                        // Examples:
                        //// 1) int a = -1; (UnaryMinusExpression) (+ version: UnaryPlusExpression)
                        //// 2) int b = ++a; (PreIncrementExpression)

                        // Could be literal positive/negative value. (-1, +5)
                        if (TryResolveExpression(pues, true, out Value v))
                        {
                            value = v;
                            succeeded = true;
                            break;
                        }

                        // Parse right side
                        if (!TryParseValue(pues.Operand, semanticModel, solution, out Value valueOper))
                        {
                            // Unable to parse the right side
                            break;
                        }

                        if (!valueOper.IsNumeric())
                        {
                            throw new NotImplementedException($"PrefixUnaryExpressionSyntax with '{valueOper.Type}' as ValueType for the Operand.");
                        }

                        if (pues.Kind() is SyntaxKind.PreIncrementExpression)
                        {
                            valueOper.Object = (dynamic)valueOper.Object + 1;
                        }

                        if (pues.Kind() is SyntaxKind.PreDecrementExpression)
                        {
                            valueOper.Object = (dynamic)valueOper.Object - 1;
                        }

                        value = valueOper;
                        succeeded = true;
                        break;
                    }

                case PostfixUnaryExpressionSyntax pues:
                    {
                        // Examples:
                        //// 1) int b = a++; (PostIncrementExpression)

                        // Parse left side
                        if (!TryParseValue(pues.Operand, semanticModel, solution, out Value valueOper))
                        {
                            // Unable to parse the left side
                            break;
                        }

                        if (!valueOper.IsNumeric())
                        {
                            throw new NotImplementedException($"PostfixUnaryExpressionSyntax with '{valueOper.Type}' as ValueType for the Operand.");
                        }

                        // TODO-MOD: Check if there are cases where we would want to have the calculated value... 
                        ////if (pues.Kind() is SyntaxKind.PostIncrementExpression)
                        ////{
                        ////    valueOper.Object = (dynamic)valueOper.Object + 1;
                        ////}

                        ////if (pues.Kind() is SyntaxKind.PostDecrementExpression)
                        ////{
                        ////    valueOper.Object = (dynamic)valueOper.Object - 1;
                        ////}

                        value = valueOper;
                        succeeded = true;
                        break;
                    }

                case BinaryExpressionSyntax bes:
                    {
                        if (TryResolveExpression(expression, true, out Value v))
                        {
                            // In case of simple basic expressions.
                            value = v;
                            succeeded = true;
                            break;
                        }

                        // When more BinaryExpressions are inside the 'main' expression, the recursive part will hopefully eventually be able to parse them.
                        if (!TryParseValue(GetExpressionFromParenthesesOrDefault(bes.Left), semanticModel, solution, out Value valueLeft))
                        {
                            // Failed to parse the left side. No point in continuing.
                            break;
                        }

                        if (!TryParseValue(GetExpressionFromParenthesesOrDefault(bes.Right), semanticModel, solution, out Value valueRight))
                        {
                            // Failed to parse the right side. No point in continuing.
                            break;
                        }

                        ExpressionSyntax exprLeft = RoslynEditor.ResolveAsLiteral(valueLeft);
                        ExpressionSyntax exprRight = RoslynEditor.ResolveAsLiteral(valueRight);

                        if (exprLeft == null || exprRight == null)
                        {
                            // Couldn't resolve the value to a literal (Example: Method argument)
                            break;
                        }

                        BinaryExpressionSyntax newExpression = SyntaxFactory.BinaryExpression(bes.Kind(), exprLeft, bes.OperatorToken, exprRight);

                        bool hasNotChanged = valueLeft.HasNotChanged && valueRight.HasNotChanged;
                        if (TryResolveExpression(newExpression, hasNotChanged, out Value finalValue))
                        {
                            // In case of simple basic expressions.
                            value = finalValue;
                            succeeded = true;
                        }

                        break;
                    }

                case CastExpressionSyntax ces:
                    {
                        if (!(ces.Type is PredefinedTypeSyntax pts))
                        {
                            // In case for casting to a class for example
                            break;
                        }

                        if (!TryParseValue(ces.Expression, semanticModel, solution, out Value v))
                        {
                            // Unable to parse the right side. No point in going further.
                            break;
                        }

                        value = v;
                        succeeded = true;

                        Value.ValueType kind = ValueTypeConverter.GetValueType(pts.Keyword.Kind());
                        value.Type = kind;

                        if (value.IsMethodArgument)
                        {
                            // No value to cast
                            break;
                        }

                        // Rebuild cast expression to resolve it
                        var literal = RoslynEditor.ResolveAsLiteral(value);
                        if (literal == null)
                        {
                            // Value can't be casted.
                            break;
                        }

                        var newCastExpression = SyntaxFactory.CastExpression(ces.Type, literal);

                        if (TryResolveExpression(newCastExpression, value.HasNotChanged, out Value newValue))
                        {
                            value.Object = newValue.Object;
                        }

                        break;
                    }

                case IdentifierNameSyntax ins:
                    {
                        if (TryGetVariableValue(ins, semanticModel, solution, out Value v))
                        {
                            value = v;
                            succeeded = true;
                        }

                        break;
                    }

                case MemberAccessExpressionSyntax maes:
                    {
                        if (TryGetVariableValue(maes, semanticModel, solution, out Value v))
                        {
                            value = v;
                            succeeded = true;
                        }

                        break;
                    }

                case ImplicitArrayCreationExpressionSyntax iaces:
                    {
                        if (TryParseValue(iaces.Initializer, semanticModel, solution, out Value v))
                        {
                            value = v;
                            succeeded = true;
                        }

                        break;
                    }

                case ArrayCreationExpressionSyntax aces:
                    {
                        if (TryParseValue(aces.Initializer, semanticModel, solution, out Value v))
                        {
                            value = v;
                            succeeded = true;
                        }

                        break;
                    }

                case InitializerExpressionSyntax ies:
                    {
                        List<Value> internalValues = new List<Value>();
                        foreach (var item in ies.Expressions)
                        {
                            if (TryParseValue(item, semanticModel, solution, out Value v))
                            {
                                internalValues.Add(v);
                            }
                            else
                            {
                                // Throw not supported exception?
                                internalValues.Add(null);
                            }
                        }

                        value = new Value
                        {
                            Type = Value.ValueType.Array,
                            Array = internalValues,
                            ArrayType = Value.ValueType.Unknown,
                            HasNotChanged = true
                        };

                        try
                        {
                            if (ies.Parent != null && (ies.Parent is ArrayCreationExpressionSyntax || ies.Parent is ImplicitArrayCreationExpressionSyntax))
                            {
                                var parentType = semanticModel.GetTypeInfo(ies.Parent).Type;
                                if (parentType is IArrayTypeSymbol ats)
                                {
                                    value.ArrayType = ValueTypeConverter.GetValueType(ats.ElementType.SpecialType);
                                }
                            }
                            else
                            {
                                var parentType = semanticModel.GetTypeInfo(ies).ConvertedType;
                                if (parentType is IArrayTypeSymbol ats)
                                {
                                    value.ArrayType = ValueTypeConverter.GetValueType(ats.ElementType.SpecialType);
                                }
                            }
                        }
                        catch (ArgumentException e)
                            when (String.Equals(e.Message, "Syntax node is not within syntax tree", StringComparison.OrdinalIgnoreCase))
                        {
                            // Can't find it in this semantic model.
                            // TODO: Check if other semantic model can be retrieved from solution? Probably extra load though...

                            // Limited support for now.
                            if (ies.Parent is ArrayCreationExpressionSyntax aes && aes.Type?.ElementType is PredefinedTypeSyntax pts)
                            {
                                value.ArrayType = ValueTypeConverter.GetValueType(pts.Keyword);
                            }
                        }

                        succeeded = true;
                        break;
                    }

                case InvocationExpressionSyntax ies:
                    {
                        bool hasNotChanged = true;

                        #region Resolve left part

                        ExpressionSyntax newExpression = ies.Expression;
                        if (ies.Expression is MemberAccessExpressionSyntax maes && TryParseValue(maes.Expression, semanticModel, solution, out Value expressionValue))
                        {
                            var temp = RoslynEditor.ResolveAsLiteral(expressionValue);

                            if (temp == null)
                            {
                                // Couldn't figure out the left part
                                return false;
                            }

                            newExpression = SyntaxFactory.MemberAccessExpression(maes.Kind(), temp, maes.OperatorToken, maes.Name);
                            hasNotChanged = expressionValue.HasNotChanged;
                        }

                        #endregion

                        #region Resolve arguments

                        List<ArgumentSyntax> newArguments = new List<ArgumentSyntax>();
                        foreach (var arg in ies.ArgumentList.Arguments)
                        {
                            if (!TryParseValue(arg.Expression, semanticModel, solution, out Value argValue))
                            {
                                // Argument couldn't be parsed. No point to continue.
                                return false;
                            }

                            var newArg = RoslynEditor.ResolveAsArgument(argValue);

                            if (newArg == null)
                            {
                                return false;
                            }

                            newArguments.Add(newArg);
                            hasNotChanged = hasNotChanged && argValue.HasNotChanged;
                        }

                        SeparatedSyntaxList<ArgumentSyntax> separatedSyntaxList = new SeparatedSyntaxList<ArgumentSyntax>();
                        separatedSyntaxList = separatedSyntaxList.AddRange(newArguments);
                        var newArgumentList = SyntaxFactory.ArgumentList(separatedSyntaxList);

                        #endregion

                        var newInvocationExpression = SyntaxFactory.InvocationExpression(newExpression, newArgumentList);

                        if (TryResolveExpression(newInvocationExpression, hasNotChanged, out Value newValue))
                        {
                            value = newValue;
                            succeeded = true;
                        }

                        break;
                    }

                case ObjectCreationExpressionSyntax oces:
                    // Examples: new DateTime(...); new MyClass();

                    value = new Value
                    {
                        Type = Value.ValueType.Unknown,
                        HasNotChanged = true,
                        Object = oces.ToFullString(),
                    };
                    succeeded = true;
                    break;

                case ConditionalExpressionSyntax ces:
                    // Examples: bool ? smth : smthElse;
                    // Expansion for the future if we expand Value to have a list of possible values.
                    break;

                default:
                    // Throw not supported exception?
                    break;
            }

            return succeeded;
        }

        /// <summary>
        /// Retrieves the expression from the parenthesized expression syntax.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>The expression from the parenthesized expression syntax.</returns>
        private static ExpressionSyntax GetExpressionFromParenthesesOrDefault(ExpressionSyntax expr)
        {
            // Get expression inside parentheses
            if (!(expr is ParenthesizedExpressionSyntax))
            {
                return expr;
            }

            foreach (SyntaxNode aux in expr.ChildNodes())
            {
                if (aux is ExpressionSyntax es)
                {
                    expr = es;
                }
            }

            // Return expression inside of parentheses or itself
            return expr;
        }

        /// <summary>
        /// Gets the fully qualified name of the specified expression.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The fully qualified name.</returns>
        public static string GetFullyQualifiedName(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            SymbolDisplayFormat symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            if (expression is MemberAccessExpressionSyntax maes)
            {
                ITypeSymbol argSymbol = semanticModel.GetTypeInfo(maes.Expression).Type;
                return argSymbol?.ToDisplayString(symbolDisplayFormat);
            }

            if (expression is InvocationExpressionSyntax ies)
            {
                ITypeSymbol argSymbol = semanticModel.GetTypeInfo(ies.Expression).Type;
                return argSymbol?.ToDisplayString(symbolDisplayFormat);
            }

            ITypeSymbol symbol = semanticModel.GetTypeInfo(expression).Type;
            return symbol?.ToDisplayString(symbolDisplayFormat);
        }
    }
}