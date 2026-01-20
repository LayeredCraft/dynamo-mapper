using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Dictionary with nested object values (inline generation)
/// </summary>
public record EmployeeDirectory
{
    public string DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public Dictionary<string, Employee> Employees { get; set; }
}

public record Employee
{
    public string Name { get; set; }
    public string Title { get; set; }
    public decimal Salary { get; set; }
}

[DynamoMapper]
public static partial class EmployeeDirectoryMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(EmployeeDirectory source);

    public static partial EmployeeDirectory FromItem(Dictionary<string, AttributeValue> item);
}