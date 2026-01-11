using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

public static partial class AttributeValueExtensions
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
        /// <remarks>
        ///     This extension property provides a convenient way to check if an AttributeValue represents
        ///     a DynamoDB NULL. It handles both the case where the AttributeValue itself is null and where it
        ///     explicitly has NULL = true.
        /// </remarks>
        public bool IsNull => attributeValue?.NULL is true;

        public bool IsNotNull => attributeValue?.NULL is null or false;
    }
}
