using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Multi-level nested objects - Company -> Department -> Manager
/// </summary>
public record Company
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Department HeadOffice { get; set; }
}

public record Department
{
    public string Code { get; set; }
    public string Name { get; set; }
    public Manager Lead { get; set; }
}

public record Manager
{
    public string EmployeeId { get; set; }
    public string FullName { get; set; }
}

[DynamoMapper]
public static partial class CompanyMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Company source);

    public static partial Company FromItem(Dictionary<string, AttributeValue> item);
}