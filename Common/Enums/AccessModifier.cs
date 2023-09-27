namespace Skyline.DataMiner.CICD.CSharpAnalysis.Enums
{
    using System;

	/// <summary>
	/// Represents access modifiers.
	/// </summary>
	/// <remarks>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/accessibility-levels"/>
	/// </remarks>
	[Flags]
    public enum AccessModifier
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,
        /// <summary>
        /// Private.
        /// </summary>
        Private = 1,
        /// <summary>
        /// Public.
        /// </summary>
        Public = 2,
        /// <summary>
        /// Protected.
        /// </summary>
        Protected = 4,
        //PrivateProtected = 5,
        /// <summary>
        /// Internal.
        /// </summary>
        Internal = 8,
        //ProtectedInteral = 12,
    }
}