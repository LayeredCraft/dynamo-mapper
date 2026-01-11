using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

internal static class KindAttributeValueExtensions
{
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
}
