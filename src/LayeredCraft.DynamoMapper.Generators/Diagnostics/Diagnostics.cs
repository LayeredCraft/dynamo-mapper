using Microsoft.CodeAnalysis;

namespace DynamoMapper.Models;

internal static class Diagnostics
{
    private const string UsageCategory = "DynamoMapper.Usage";

    internal static readonly DiagnosticDescriptor CannotConvertFromAttributeValue = new(
        "DM0001",
        "Type cannot be mapped to an AttributeValue",
        "The property '{0}' of type '{1}' cannot be mapped to an AttributeValue",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor CannotConvertToAttributeValue = new(
        "DM0002",
        "Type cannot be mapped from an AttributeValue",
        "The property '{0}' of type '{1}' cannot be mapped from an AttributeValue",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );
}
