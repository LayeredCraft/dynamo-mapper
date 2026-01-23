using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Multi-level nested objects - Company -> Department -> Manager
/// </summary>
public record Company
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required Department HeadOffice { get; set; }
}

public record Department
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required Manager Lead { get; set; }
}

public record Manager
{
    public required string EmployeeId { get; set; }
    public required string FullName { get; set; }
}

[DynamoMapper]
public static partial class CompanyMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Company source);

    public static partial Company FromItem(Dictionary<string, AttributeValue> item);
}
