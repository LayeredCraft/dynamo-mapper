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

// ── Mapper ─────────────────────────────────────────────────────────────────────

[DynamoMapper]
public static partial class DeepNestedAccountMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(DeepNestedAccount source);

    public static partial DeepNestedAccount FromItem(Dictionary<string, AttributeValue> item);
}
