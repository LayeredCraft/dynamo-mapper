using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>Shared helpers for AttributeValue extension methods.</summary>
internal static class UtilAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>
        ///     Retrieves an attribute value from the dictionary, returning a NULL AttributeValue if the
        ///     key is missing and the attribute is optional.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <returns>
        ///     The <see cref="AttributeValue" /> if the key exists; otherwise a NULL
        ///     <see cref="AttributeValue" /> if <paramref name="requiredness" /> is
        ///     <see cref="Requiredness.Optional" /> or <see cref="Requiredness.InferFromNullability" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the key is missing and
        ///     <paramref name="requiredness" /> is <see cref="Requiredness.Required" />.
        /// </exception>
        /// <remarks>
        ///     This helper is used by <c>GetNullable*</c> methods to handle missing attributes. When the
        ///     key is missing and the attribute is optional, it returns an AttributeValue with NULL = true,
        ///     allowing the caller to distinguish between a missing attribute and an explicit DynamoDB NULL
        ///     value.
        /// </remarks>
        internal AttributeValue GetNullableValue(string key, Requiredness requiredness)
        {
            if (!attributes.TryGetValue(key, out var attributeValue))
                return requiredness switch
                {
                    Requiredness.Required => throw new InvalidOperationException(
                        $"The DynamoDB item does not contain an attribute named '{key}'."
                    ),
                    Requiredness.Optional or Requiredness.InferFromNullability => new AttributeValue
                    {
                        NULL = true,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(requiredness)),
                };

            return attributeValue;
        }

        /// <summary>Attempts to retrieve an attribute value from the dictionary with requiredness validation.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="value">
        ///     When this method returns, contains the <see cref="AttributeValue" /> if the key
        ///     exists; otherwise <c>null</c> if the key is missing and <paramref name="requiredness" /> is
        ///     <see cref="Requiredness.Optional" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the key exists in the dictionary; otherwise <c>false</c> if the key is
        ///     missing and <paramref name="requiredness" /> is <see cref="Requiredness.Optional" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the key is missing and
        ///     <paramref name="requiredness" /> is <see cref="Requiredness.Required" /> or
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </exception>
        /// <remarks>
        ///     This helper is used by non-nullable <c>Get*</c> methods to handle missing attributes.
        ///     Unlike <see cref="GetNullableValue" />, this method returns <c>false</c> with a <c>null</c> out
        ///     parameter when the key is missing and the attribute is optional, allowing callers to provide
        ///     default values for non-nullable types.
        /// </remarks>
        internal bool TryGetValue(string key, Requiredness requiredness, out AttributeValue? value)
        {
            value = null;

            if (!attributes.TryGetValue(key, out var attributeValue))
            {
                value = requiredness switch
                {
                    Requiredness.Required or Requiredness.InferFromNullability =>
                        throw new InvalidOperationException(
                            $"The DynamoDB item does not contain an attribute named '{key}'."
                        ),
                    Requiredness.Optional => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(requiredness)),
                };

                return false;
            }

            value = attributeValue;
            return true;
        }

        internal AttributeValue GetValue(string key, Requiredness requiredness)
        {
            if (!attributes.TryGetValue(key, out var attributeValue))
                return requiredness switch
                {
                    Requiredness.Required or Requiredness.InferFromNullability =>
                        throw new InvalidOperationException(
                            $"The DynamoDB item does not contain an attribute named '{key}'."
                        ),
                    Requiredness.Optional => new AttributeValue { NULL = true },
                    _ => throw new ArgumentOutOfRangeException(nameof(requiredness)),
                };

            return attributeValue;
        }
    }

    extension(AttributeValue? attributeValue)
    {
        /// <summary>
        ///     Gets a value indicating whether this <see cref="AttributeValue" /> represents a DynamoDB
        ///     NULL value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the attribute value is <c>null</c> or has its
        ///     <see cref="AttributeValue.NULL" /> property set to <c>true</c>; otherwise <c>false</c>.
        /// </value>
        /// <remarks>
        ///     This extension property provides a convenient way to check if an AttributeValue represents
        ///     a DynamoDB NULL. It handles both the case where the AttributeValue itself is null and where it
        ///     explicitly has NULL = true.
        /// </remarks>
        public bool IsNull => attributeValue?.NULL is true;

        public bool IsNotNull => attributeValue?.NULL is null or false;
    }

    extension(AttributeValue attributeValue)
    {
        public string GetString(DynamoKind kind) =>
            kind switch
            {
                DynamoKind.S => attributeValue.S,
                DynamoKind.N => attributeValue.N,
                DynamoKind.BOOL => attributeValue.BOOL.ToString(),
                DynamoKind.B
                or DynamoKind.M
                or DynamoKind.L
                or DynamoKind.NULL
                or DynamoKind.SS
                or DynamoKind.NS
                or DynamoKind.BS => throw new NotImplementedException(
                    $"Not implemented for kind: {kind}"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            } ?? string.Empty;

        public string? GetNullableString(DynamoKind kind) =>
            kind switch
            {
                _ when attributeValue.NULL is true => null,
                DynamoKind.S => attributeValue.S,
                DynamoKind.N => attributeValue.N,
                DynamoKind.BOOL => attributeValue.BOOL.ToString(),
                DynamoKind.B
                or DynamoKind.M
                or DynamoKind.L
                or DynamoKind.NULL
                or DynamoKind.SS
                or DynamoKind.NS
                or DynamoKind.BS => throw new NotImplementedException(
                    $"Not implemented for kind: {kind}"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };

        public bool GetBool(DynamoKind kind) =>
            kind switch
            {
                DynamoKind.S => bool.Parse(attributeValue.S),
                DynamoKind.N => bool.Parse(attributeValue.N),
                DynamoKind.BOOL => attributeValue.BOOL ?? false,
                DynamoKind.B
                or DynamoKind.M
                or DynamoKind.L
                or DynamoKind.NULL
                or DynamoKind.SS
                or DynamoKind.NS
                or DynamoKind.BS => throw new NotImplementedException(
                    $"Not implemented for kind: {kind}"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };

        public bool? GetNullableBool(DynamoKind kind) =>
            kind switch
            {
                _ when attributeValue.NULL is true => null,
                DynamoKind.S => bool.Parse(attributeValue.S),
                DynamoKind.N => bool.Parse(attributeValue.N),
                DynamoKind.BOOL => attributeValue.BOOL,
                DynamoKind.B
                or DynamoKind.M
                or DynamoKind.L
                or DynamoKind.NULL
                or DynamoKind.SS
                or DynamoKind.NS
                or DynamoKind.BS => throw new NotImplementedException(
                    $"Not implemented for kind: {kind}"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };
    }

    extension(bool? value)
    {
        internal bool ShouldSet(bool omitNullStrings) =>
            value switch
            {
                null when omitNullStrings => false,
                _ => true,
            };
    }

    extension(string? value)
    {
        internal bool ShouldSet(bool omitEmptyStrings, bool omitNullStrings) =>
            value switch
            {
                null when omitNullStrings => false,
                { Length: 0 } when omitEmptyStrings => false,
                _ => true,
            };
    }
}
