using LayeredCraft.DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>Extension methods for binary and stream values stored as DynamoDB <c>B</c> attributes.</summary>
public static class BinaryAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Tries to get a binary value from the attribute dictionary.</summary>
        public bool TryGetBinary(
            string key, out byte[] value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        )
        {
            value = [];
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            value = GetBinaryBytes(attribute!, kind);
            return true;
        }

        /// <summary>Gets a binary value from the attribute dictionary.</summary>
        public byte[] GetBinary(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        ) => attributes.TryGetBinary(key, out var value, requiredness, kind) ? value : [];

        /// <summary>Tries to get a nullable binary value from the attribute dictionary.</summary>
        public bool TryGetNullableBinary(
            string key, out byte[]? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            if (attribute.IsNull)
                return true;

            value = GetBinaryBytes(attribute!, kind);
            return true;
        }

        /// <summary>Gets a nullable binary value from the attribute dictionary.</summary>
        public byte[]? GetNullableBinary(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        ) => attributes.TryGetNullableBinary(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Sets a binary value in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetBinary(
            string key, byte[]? value, bool omitEmptyStrings = false, bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.B
        )
        {
            if (value is null)
            {
                if (!omitNullStrings)
                    attributes[key] = new AttributeValue { NULL = true };

                return attributes;
            }

            if (value.Length == 0 && omitEmptyStrings)
                return attributes;

            attributes[key] = ToBinaryAttributeValue(value, kind);
            return attributes;
        }

        /// <summary>Tries to get a stream value from the attribute dictionary.</summary>
        public bool TryGetStream(
            string key, out Stream? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        )
        {
            value = null;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            value = CreateBinaryMemoryStream(attribute!, kind);
            return true;
        }

        /// <summary>Gets a stream value from the attribute dictionary.</summary>
        public Stream GetStream(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        ) => attributes.TryGetStream(key, out var value, requiredness, kind)
            ? value!
            : new MemoryStream();

        /// <summary>Tries to get a nullable stream value from the attribute dictionary.</summary>
        public bool TryGetNullableStream(
            string key, out Stream? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            if (attribute.IsNull)
                return true;

            value = CreateBinaryMemoryStream(attribute!, kind);
            return true;
        }

        /// <summary>Gets a nullable stream value from the attribute dictionary.</summary>
        public Stream? GetNullableStream(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.B
        ) => attributes.TryGetNullableStream(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Sets a stream value in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetStream(
            string key, Stream? value, bool omitEmptyStrings = false, bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.B
        )
        {
            if (value is null)
            {
                if (!omitNullStrings)
                    attributes[key] = new AttributeValue { NULL = true };

                return attributes;
            }

            var streamValue = CreateBinaryMemoryStream(value);
            if (streamValue.Length == 0 && omitEmptyStrings)
                return attributes;

            attributes[key] = ToBinaryAttributeValue(streamValue, kind);
            return attributes;
        }
    }

    internal static byte[] GetBinaryBytes(AttributeValue attributeValue, DynamoKind kind)
    {
        if (attributeValue is null)
            throw new ArgumentNullException(nameof(attributeValue));

        if (kind != DynamoKind.B)
            throw new NotImplementedException(
                $"{nameof(GetBinaryBytes)} is not implemented for kind: {kind}"
            );

        return attributeValue.B?.ToArray() ?? [];
    }

    internal static MemoryStream CreateBinaryMemoryStream(
        AttributeValue attributeValue, DynamoKind kind
    ) => new(GetBinaryBytes(attributeValue, kind), false);

    internal static MemoryStream CreateBinaryMemoryStream(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        var originalPosition = stream.CanSeek ? stream.Position : 0;
        if (stream.CanSeek)
            stream.Position = originalPosition;

        var copy = new MemoryStream();
        stream.CopyTo(copy);
        copy.Position = 0;

        if (stream.CanSeek)
            stream.Position = originalPosition;

        return copy;
    }

    internal static AttributeValue ToBinaryAttributeValue(byte[] value, DynamoKind kind)
    {
        if (kind != DynamoKind.B)
            throw new NotImplementedException(
                $"{nameof(ToBinaryAttributeValue)} is not implemented for kind: {kind}"
            );

        return new AttributeValue { B = new MemoryStream(value, false) };
    }

    internal static AttributeValue ToBinaryAttributeValue(Stream value, DynamoKind kind)
    {
        if (kind != DynamoKind.B)
            throw new NotImplementedException(
                $"{nameof(ToBinaryAttributeValue)} is not implemented for kind: {kind}"
            );

        return new AttributeValue { B = CreateBinaryMemoryStream(value) };
    }
}
