using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>Extension methods for <see cref="AttributeValue" /> null handling.</summary>
public static class AttributeValueExtensions
{
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
        public bool IsNull => attributeValue?.NULL is true;

        /// <summary>
        ///     Gets a value indicating whether this <see cref="AttributeValue" /> represents a non-NULL
        ///     value.
        /// </summary>
        /// <value><c>true</c> if the attribute value is not a DynamoDB NULL value; otherwise <c>false</c>.</value>
        public bool IsNotNull => attributeValue?.NULL is null or false;
    }

    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Sets an <see cref="AttributeValue" /> in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="AttributeValue" /> to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> Set(string key, AttributeValue value)
        {
            attributes[key] = value;

            return attributes;
        }
    }
}
