using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>Shared helpers for AttributeValue extension methods.</summary>
internal static class UtilAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
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
                    $"{nameof(GetString)} is not implemented for kind: {kind}"
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
                    $"{nameof(GetNullableString)} is not implemented for kind: {kind}"
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
                    $"{nameof(GetBool)} is not implemented for kind: {kind}"
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
                    $"{nameof(GetNullableBool)} is not implemented for kind: {kind}"
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

        internal AttributeValue ToAttributeValue(DynamoKind kind) =>
            kind switch
            {
                _ when value is null => new AttributeValue { NULL = true },
                DynamoKind.S => new AttributeValue { S = value.ToString() },
                DynamoKind.N => new AttributeValue { N = value.ToString() },
                DynamoKind.BOOL => new AttributeValue { BOOL = value.Value },
                DynamoKind.B
                or DynamoKind.M
                or DynamoKind.L
                or DynamoKind.NULL
                or DynamoKind.SS
                or DynamoKind.NS
                or DynamoKind.BS => throw new NotImplementedException(
                    $"{nameof(ToAttributeValue)} is not implemented for kind: {kind}"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
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

        internal AttributeValue ToAttributeValue(DynamoKind kind) =>
            kind switch
            {
                _ when value is null => new AttributeValue { NULL = true },
                DynamoKind.S => new AttributeValue { S = value },
                DynamoKind.N => new AttributeValue { N = value },
                DynamoKind.BOOL => new AttributeValue { BOOL = bool.Parse(value) },
                DynamoKind.B
                or DynamoKind.M
                or DynamoKind.L
                or DynamoKind.NULL
                or DynamoKind.SS
                or DynamoKind.NS
                or DynamoKind.BS => throw new NotImplementedException(
                    $"{nameof(ToAttributeValue)} is not implemented for kind: {kind}"
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };
    }
}
