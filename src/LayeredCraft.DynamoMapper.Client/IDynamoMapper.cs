using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoClient;

/// <summary>Maps a DTO to and from a DynamoDB item representation.</summary>
/// <typeparam name="TDto">The DTO type handled by the mapper.</typeparam>
public interface IDynamoMapper<TDto> where TDto : class
{
    /// <summary>Converts a DTO instance into a DynamoDB item.</summary>
    /// <param name="source">The DTO instance to convert.</param>
    /// <returns>A DynamoDB item keyed by attribute name.</returns>
    static abstract Dictionary<string, AttributeValue> ToItem(TDto source);

    /// <summary>Creates a DTO instance from a DynamoDB item.</summary>
    /// <param name="item">The DynamoDB item to convert.</param>
    /// <returns>The DTO created from the item.</returns>
    static abstract TDto FromItem(Dictionary<string, AttributeValue> item);
}
