using Microsoft.CodeAnalysis;

namespace DynamoMapper.Models;

internal static class Diagnostics
{
    private const string UsageCategory = "DynamoMapper.Usage";

    internal static readonly DiagnosticDescriptor CannotConvertFromAttributeValue = new(
        "DM0001",
        "Type cannot be converted from AttributeValue",
        "The type '{0}' cannot be converted from converted from an AttributeValue",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor CannotConvertToAttributeValue = new(
        "DM0002",
        "Type cannot be converted to AttributeValue",
        "The type '{0}' cannot be converted from converted to an AttributeValue",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );
}
