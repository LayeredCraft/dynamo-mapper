using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Nested object with various scalar types
/// </summary>
public record Invoice
{
    public string InvoiceNumber { get; set; }
    public PaymentInfo Payment { get; set; }
}

public record PaymentInfo
{
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public bool IsRefundable { get; set; }
    public int InstallmentCount { get; set; }
}

[DynamoMapper]
public static partial class InvoiceMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Invoice source);

    public static partial Invoice FromItem(Dictionary<string, AttributeValue> item);
}