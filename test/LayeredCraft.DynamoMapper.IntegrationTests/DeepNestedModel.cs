using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

// ── Domain models ──────────────────────────────────────────────────────────────
// Shape: Account → UserProfile (owned) → List<PurchaseOrder> (complex list)
//                                      → PurchaseOrder → ShippingAddress (owned)
//                                                      → List<LineItem> (complex list)

public sealed class DeepNestedLineItem
{
    public string Sku { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal Price { get; set; }
}

public sealed class DeepNestedShippingAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public sealed class DeepNestedPurchaseOrder
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DeepNestedShippingAddress Destination { get; set; } = new();
    public List<DeepNestedLineItem> Lines { get; set; } = [];
}

public sealed class DeepNestedUserProfile
{
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public List<DeepNestedPurchaseOrder> Orders { get; set; } = [];
}

public sealed class DeepNestedAccount
{
    public string AccountId { get; set; } = string.Empty;
    public DeepNestedUserProfile Profile { get; set; } = new();
}

// ── 4-level model ──────────────────────────────────────────────────────────────
// Shape: Org → Division (owned) → List<Department> → Manager (owned) + List<Employee>

public sealed class FourLevelEmployee
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
}

public sealed class FourLevelManager
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class FourLevelDepartment
{
    public string Code { get; set; } = string.Empty;
    public FourLevelManager HeadManager { get; set; } = new();
    public List<FourLevelEmployee> Employees { get; set; } = [];
}

public sealed class FourLevelDivision
{
    public string Name { get; set; } = string.Empty;
    public List<FourLevelDepartment> Departments { get; set; } = [];
}

public sealed class FourLevelOrg
{
    public string OrgId { get; set; } = string.Empty;
    public FourLevelDivision Division { get; set; } = new();
}

// ── Mappers ────────────────────────────────────────────────────────────────────

[DynamoMapper]
public static partial class DeepNestedAccountMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(DeepNestedAccount source);

    public static partial DeepNestedAccount FromItem(Dictionary<string, AttributeValue> item);
}

[DynamoMapper]
public static partial class FourLevelOrgMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(FourLevelOrg source);

    public static partial FourLevelOrg FromItem(Dictionary<string, AttributeValue> item);
}
