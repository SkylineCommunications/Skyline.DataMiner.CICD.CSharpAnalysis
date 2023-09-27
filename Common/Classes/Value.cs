namespace Skyline.DataMiner.CICD.CSharpAnalysis.Classes
{
    using System;
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a value.
    /// </summary>
    public class Value : CSharpObject<SyntaxNode>
    {
        /// <summary>
        /// Gets the symbol.
        /// </summary>
        /// <value>The symbol.</value>
        public ISymbol _Symbol { get; internal set; }

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <value>The object.</value>
        public object Object { get; internal set; }

        /// <summary>
        /// Gets the array.
        /// </summary>
        /// <value>The array.</value>
        /// <remarks>Items inside the Array can be null.</remarks>
        public IReadOnlyList<Value> Array { get; internal set; }

        /// <summary>
        /// Gets the array type.
        /// </summary>
        /// <value>The array type.</value>
        public ValueType ArrayType { get; internal set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public ValueType Type { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this is a method argument.
        /// </summary>
        /// <value><c>true</c> if this is a method argument; otherwise, <c>false</c>.</value>
        public bool IsMethodArgument { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the value has changed.
        /// </summary>
        /// <value><c>true</c> if the value has changed; otherwise, <c>false</c>.</value>
        public bool HasNotChanged { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this has a static value.
        /// </summary>
        /// <value><c>true</c> if this has a static value; otherwise, <c>false</c>.</value>
        public bool HasStaticValue => HasNotChanged && !IsMethodArgument;

        /// <summary>
        /// Converts the object value to an integer or <see langword="null"/> if the value does not represent an integer value.
        /// </summary>
        public int AsInt32 => Int32.TryParse(Convert.ToString(Object), out int r) ? r : default;

        /// <summary>
        /// Gets a value indicating whether this is a numeric value.
        /// </summary>
        /// <value><c>true</c> if this is a numeric value; otherwise, <c>false</c>.</value>
        public bool IsNumeric()
        {
            bool integers = Type == ValueType.Int8 || Type == ValueType.Int16 || Type == ValueType.Int32 || Type == ValueType.Int64;
            bool unsignedIntegers = Type == ValueType.UInt8 || Type == ValueType.UInt16 || Type == ValueType.UInt32 || Type == ValueType.UInt64;
            bool others = Type == ValueType.Single || Type == ValueType.Double || Type == ValueType.Decimal;

            return integers || unsignedIntegers || others;
        }

        /// <summary>
        /// Converts the value to a string.
        /// </summary>
        /// <returns>The string value.</returns>
        public string GetValueTypeAsString()
        {
            if (Type == ValueType.Array)
            {
                string arrayTypeAsString = ValueTypeConverter.ToString(ArrayType);
                return $"{arrayTypeAsString}[]";
            }

            return ValueTypeConverter.ToString(Type);
        }

        /// <summary>
        /// Represents the value type.
        /// </summary>
        public enum ValueType
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            Unknown = -1,
            /// <summary>
            /// <see langword="null"/>.
            /// </summary>
            Null,
            /// <summary>
            /// Object.
            /// </summary>
            Object,
            /// <summary>
            /// string
            /// </summary>
            String,
            /// <summary>
            /// SByte
            /// </summary>
            Int8,
            /// <summary>
            /// Short
            /// </summary>
            Int16,
            /// <summary>
            /// Int
            /// </summary>
            Int32,
            /// <summary>
            /// Long
            /// </summary>
            Int64,
            /// <summary>
            /// Byte
            /// </summary>
            UInt8,
            /// <summary>
            /// UShort
            /// </summary>
            UInt16,
            /// <summary>
            /// Uint
            /// </summary>
            UInt32,
            /// <summary>
            /// ULong
            /// </summary>
            UInt64,
            /// <summary>
            /// Float
            /// </summary>
            Single,
            /// <summary>
            /// Double
            /// </summary>
            Double,
            /// <summary>
            /// Decimal
            /// </summary>
            Decimal,
            /// <summary>
            /// Array
            /// </summary>
            Array,
            /// <summary>
            /// Boolean.
            /// </summary>
            Boolean,
            /// <summary>
            /// Date time.
            /// </summary>
            DateTime,
        }
    }
}