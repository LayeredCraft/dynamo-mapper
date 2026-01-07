namespace DynamoMapper.Runtime;

/// <summary>Naming convention for mapping .NET property names to DynamoDB attribute names.</summary>
public enum DynamoNamingConvention
{
    /// <summary>Property name is used exactly as-is (e.g., ProductId → ProductId).</summary>
    Exact,

    /// <summary>Property name is converted to camelCase (e.g., ProductId → productId).</summary>
    CamelCase,

    /// <summary>Property name is converted to PascalCase (e.g., productId → ProductId).</summary>
    PascalCase,

    /// <summary>Property name is converted to snake_case (e.g., ProductId → product_id).</summary>
    SnakeCase,

    /// <summary>Property name is converted to kebab-case (e.g., ProductId → product-id).</summary>
    KebabCase,
}
