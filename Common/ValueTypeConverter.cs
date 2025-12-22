namespace Skyline.DataMiner.CICD.CSharpAnalysis
{
    using System;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    using Skyline.DataMiner.CICD.CSharpAnalysis.Classes;

    /// <summary>
    /// Value type converter.
    /// </summary>
    public static class ValueTypeConverter
    {
        /// <summary>
        /// Converts the specified value to a string.
        /// </summary>
        /// <param name="valueType">The value type to convert.</param>
        /// <returns></returns>
        public static string ToString(Value.ValueType valueType)
        {
            switch (valueType)
            {
                case Value.ValueType.Null: return "null";
                case Value.ValueType.Object: return "object";
                case Value.ValueType.String: return "string";
                case Value.ValueType.Int8: return "sbyte";
                case Value.ValueType.Int16: return "short";
                case Value.ValueType.Int32: return "int";
                case Value.ValueType.Int64: return "long";
                case Value.ValueType.UInt8: return "byte";
                case Value.ValueType.UInt16: return "ushort";
                case Value.ValueType.UInt32: return "uint";
                case Value.ValueType.UInt64: return "ulong";
                case Value.ValueType.Single: return "float";
                case Value.ValueType.Double: return "double";
                case Value.ValueType.Decimal: return "decimal";
                case Value.ValueType.Boolean: return "bool";
                case Value.ValueType.DateTime: return "DateTime";

                default: return String.Empty;
            }
        }

        /// <summary>
        /// Retrieves the value type of the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The value type.</returns>
        public static Value.ValueType GetValueType(SyntaxToken token)
        {
            var valueType = GetValueType(token.Kind());
            if (valueType == Value.ValueType.Unknown)
            {
                // Try based on the actual value
                // In certain cases, the type via the value is string even though it is an uint[]. But the kind is UintKeyword which is more accurate.
                valueType = GetValueType(token.Value?.GetType());
            }

            return valueType;
        }

        /// <summary>
        /// Retrieves the ValueType of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The value type.</returns>
        public static Value.ValueType GetValueType(Type type)
        {
            switch (type?.FullName)
            {
                case "System.String": return Value.ValueType.String;
                case "System.SByte": return Value.ValueType.Int8;
                case "System.Int16": return Value.ValueType.Int16;
                case "System.Int32": return Value.ValueType.Int32;
                case "System.Int64": return Value.ValueType.Int64;
                case "System.Byte": return Value.ValueType.UInt8;
                case "System.UInt16": return Value.ValueType.UInt16;
                case "System.UInt32": return Value.ValueType.UInt32;
                case "System.UInt64": return Value.ValueType.UInt64;
                case "System.Single": return Value.ValueType.Single;
                case "System.Double": return Value.ValueType.Double;
                case "System.Decimal": return Value.ValueType.Decimal;
                case "System.Object": return Value.ValueType.Object;
                case "System.Boolean": return Value.ValueType.Boolean;
                case "System.DateTime": return Value.ValueType.DateTime;
                case null: return Value.ValueType.Null;
                default: return Value.ValueType.Unknown;
            }
        }

        /// <summary>
        /// Retrieves the value type of the specified syntax kind.
        /// </summary>
        /// <param name="kind">The syntax kind.</param>
        /// <returns>The value type.</returns>
        public static Value.ValueType GetValueType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StringLiteralToken: return Value.ValueType.String;
                case SyntaxKind.StringKeyword: return Value.ValueType.String;

                case SyntaxKind.SByteKeyword: return Value.ValueType.Int8;
                case SyntaxKind.ShortKeyword: return Value.ValueType.Int16;
                case SyntaxKind.IntKeyword: return Value.ValueType.Int32;
                case SyntaxKind.LongKeyword: return Value.ValueType.Int64;

                case SyntaxKind.ByteKeyword: return Value.ValueType.UInt8;
                case SyntaxKind.UShortKeyword: return Value.ValueType.UInt16;
                case SyntaxKind.UIntKeyword: return Value.ValueType.UInt32;
                case SyntaxKind.ULongKeyword: return Value.ValueType.UInt64;

                case SyntaxKind.FloatKeyword: return Value.ValueType.Single;
                case SyntaxKind.DoubleKeyword: return Value.ValueType.Double;
                case SyntaxKind.DecimalKeyword: return Value.ValueType.Decimal;

                case SyntaxKind.ObjectKeyword: return Value.ValueType.Object;
                case SyntaxKind.NullKeyword: return Value.ValueType.Null;
                default: return Value.ValueType.Unknown;
            }
        }

        /// <summary>
        /// Retrieves the syntax kind of the specified value type.
        /// </summary>
        /// <param name="valueType">The value type.</param>
        /// <returns>The syntax kind.</returns>
        public static SyntaxKind GetSyntaxKind(Value.ValueType valueType)
        {
            switch (valueType)
            {
                case Value.ValueType.String: return SyntaxKind.StringKeyword;
                case Value.ValueType.Int8: return SyntaxKind.SByteKeyword;
                case Value.ValueType.Int16: return SyntaxKind.ShortKeyword;
                case Value.ValueType.Int32: return SyntaxKind.IntKeyword;
                case Value.ValueType.Int64: return SyntaxKind.LongKeyword;
                case Value.ValueType.UInt8: return SyntaxKind.ByteKeyword;
                case Value.ValueType.UInt16: return SyntaxKind.UShortKeyword;
                case Value.ValueType.UInt32: return SyntaxKind.UIntKeyword;
                case Value.ValueType.UInt64: return SyntaxKind.ULongKeyword;
                case Value.ValueType.Single: return SyntaxKind.FloatKeyword;
                case Value.ValueType.Double: return SyntaxKind.DoubleKeyword;
                case Value.ValueType.Decimal: return SyntaxKind.DecimalKeyword;
                case Value.ValueType.Object: return SyntaxKind.ObjectKeyword;
                case Value.ValueType.Null: return SyntaxKind.NullKeyword;

                default: return SyntaxKind.None;
            }
        }

        /// <summary>
        /// Retrieves the value type of the specified special type.
        /// </summary>
        /// <param name="specialType">The special type.</param>
        /// <returns>The value type.</returns>
        public static Value.ValueType GetValueType(SpecialType specialType)
        {
            switch (specialType)
            {
                // TODO: See to maybe also support this?
                case SpecialType.System_Enum:
                case SpecialType.System_Char:
                    return Value.ValueType.Unknown;

                case SpecialType.System_Object: return Value.ValueType.Object;
                case SpecialType.System_Boolean: return Value.ValueType.Boolean;
                case SpecialType.System_SByte: return Value.ValueType.Int8;
                case SpecialType.System_Byte: return Value.ValueType.UInt8;
                case SpecialType.System_Int16: return Value.ValueType.Int16;
                case SpecialType.System_UInt16: return Value.ValueType.UInt16;
                case SpecialType.System_Int32: return Value.ValueType.Int32;
                case SpecialType.System_UInt32: return Value.ValueType.UInt32;
                case SpecialType.System_Int64: return Value.ValueType.Int64;
                case SpecialType.System_UInt64: return Value.ValueType.UInt64;
                case SpecialType.System_Decimal: return Value.ValueType.Decimal;
                case SpecialType.System_Single: return Value.ValueType.Single;
                case SpecialType.System_Double: return Value.ValueType.Double;
                case SpecialType.System_String: return Value.ValueType.String;
                case SpecialType.System_DateTime: return Value.ValueType.DateTime;

                default:
                    return Value.ValueType.Unknown;
            }
        }
    }
}