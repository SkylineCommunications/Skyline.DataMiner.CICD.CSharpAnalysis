namespace Skyline.DataMiner.CICD.CSharpAnalysis.Protocol
{
	using System;

	using Microsoft.CodeAnalysis;

	using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

	/// <summary>
	/// Helper class containing utility methods for analyzing QActions.
	/// </summary>
	public static class QActionAnalysisHelpers
    {
        /// <summary>
        /// Determines whether the value is of the Parameter type of the QAction_Helper.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="solution">The solution.</param>
        /// <returns><c>true</c> if the value is of the Parameter type; otherwise, <c>false</c>.</returns>
        public static bool IsParameterClass(this Value value, SemanticModel semanticModel, Solution solution)
        {
            return RoslynHelper.CheckIfCertainClass(value._Symbol, semanticModel, solution, "QAction_Helper", "Skyline.DataMiner.Scripting.Parameter");
        }

		/// <summary>
		/// Determines whether the calling method is called upon a QActionTable.
		/// </summary>
		/// <param name="callingMethod">The calling method.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <returns><c>true</c> if the calling method is called upon a QActionTable; otherwise, <c>false</c>.</returns>
		public static bool IsQActionTable(this CallingMethodClass callingMethod, SemanticModel semanticModel)
        {
            string parentClassName = callingMethod.GetFullyQualifiedNameOfParent(semanticModel);
            if (parentClassName == null)
            {
                return false;
            }

            return parentClassName.StartsWith("Skyline.DataMiner.Scripting.") && parentClassName.Contains("QActionTable");
        }

		/// <summary>
		/// Determines whether the calling method is the NotifyDataMiner method.
		/// </summary>
		/// <param name="callingMethod">The calling method.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="solution">The solution.</param>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if the calling method is the NotifyProtocol method; otherwise, <c>false</c>.</returns>
		public static bool IsNotifyProtocol(this CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution, int type)
        {
            if (!callingMethod.IsSLProtocol(semanticModel) || !String.Equals(callingMethod.Name, "NotifyProtocol"))
            {
                return false;
            }

            if (callingMethod.Arguments.Count != 3)
            {
                // Invalid amount of arguments (NotifyProtocol always has 3 arguments)
                return false;
            }

            if (!callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value) || !value.IsNumeric() || !value.HasStaticValue)
            {
                // Couldn't be parsed or isn't a numeric value
                return false;
            }

            return value.AsInt32 == type;
        }

		/// <summary>
		/// Determines whether the calling method is the NotifyDataMiner method.
		/// </summary>
		/// <param name="callingMethod">The calling method.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="solution">The solution.</param>
		/// <param name="type">The type.</param>
		/// <param name="checkQueued"><c>true</c> to also check the queued variant of the Notify method; otherwise, <c>false</c>.</param>
		/// <returns><c>true</c> if the calling method is the NotifyDataMiner(Queued) method; otherwise, <c>false</c>.</returns>
		public static bool IsNotifyDataMiner(this CallingMethodClass callingMethod, SemanticModel semanticModel, Solution solution, int type, bool checkQueued = true)
        {
            if (!callingMethod.IsSLProtocol(semanticModel))
            {
                return false;
            }

            if (checkQueued)
            {
                if (!String.Equals(callingMethod.Name, "NotifyDataMiner") && !String.Equals(callingMethod.Name, "NotifyDataMinerQueued"))
                {
                    return false;
                }
            }
            else
            {
                if (!String.Equals(callingMethod.Name, "NotifyDataMiner"))
                {
                    return false;
                }
            }

            if (callingMethod.Arguments.Count != 3)
            {
                // Invalid amount of arguments (NotifyDataMiner always has 3 arguments)
                return false;
            }

            if (!callingMethod.Arguments[0].TryParseToValue(semanticModel, solution, out Value value) || !value.IsNumeric() || !value.HasStaticValue)
            {
                // Couldn't be parsed or isn't a numeric value
                return false;
            }

            return value.AsInt32 == type;
        }

		/// <summary>
		/// Checks if the calling method is called upon the SLProtocol or SLProtocolExt interface.
		/// </summary>
		/// <param name="callingMethod">The calling method.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <returns><c>true</c> if the calling method is called upon the SLProtocol or SLProtocolExt interface; otherwise, <c>false</c>.</returns>
		public static bool IsSLProtocol(this CallingMethodClass callingMethod, SemanticModel semanticModel)
        {
            string fqn = callingMethod.GetFullyQualifiedNameOfParent(semanticModel);
            
            return String.Equals(fqn, "Skyline.DataMiner.Scripting.SLProtocol") ||
                   String.Equals(fqn, "Skyline.DataMiner.Scripting.SLProtocolExt");
        }

		/// <summary>
		/// Checks if the parameter/argument of a method is the SLProtocol or SLProtocolExt interface.
		/// </summary>
		/// <param name="parameterClass">Parameter/argument of a method.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <returns><c>true</c> if the parameter/argument of a method is the SLProtocol or SLProtocolExt interface; otherwise, <c>false</c>.</returns>
		public static bool IsSLProtocol(this ParameterClass parameterClass, SemanticModel semanticModel)
        {
            string fqn = RoslynHelper.GetFullyQualifiedName(semanticModel, parameterClass.SyntaxNode.Type);
            return String.Equals(fqn, "Skyline.DataMiner.Scripting.SLProtocol") ||
                   String.Equals(fqn, "Skyline.DataMiner.Scripting.SLProtocolExt");
        }
    }
}