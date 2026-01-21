using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Diagnostics;

internal static class DiagnosticDescriptors
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

    internal static readonly DiagnosticDescriptor UnsupportedCollectionElementType = new(
        "DM0003",
        "Collection element type not supported",
        "The property '{0}' has element type '{1}' which is not supported. Only primitive types are supported as collection elements.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor DictionaryKeyMustBeString = new(
        "DM0004",
        "Dictionary key must be string",
        "The property '{0}' has key type '{1}' but dictionary keys must be of type 'string'",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor IncompatibleKindOverride = new(
        "DM0005",
        "Incompatible DynamoKind override for collection",
        "The property '{0}' has Kind override '{1}' which is incompatible with the inferred collection kind '{2}'",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor NoMapperMethodsFound = new(
        "DM0101",
        "No mapper methods found",
        "The mapper class '{0}' must define at least one partial method starting with 'To' or 'From'",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor MismatchedPocoTypes = new(
        "DM0102",
        "Mapper methods use different POCO types",
        "The mapper methods '{0}' and '{1}' must use the same POCO type, but '{2}' and '{3}' were found",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor MultipleConstructorsWithAttribute = new(
        "DM0103",
        "Multiple constructors marked with [DynamoMapperConstructor]",
        "The type '{0}' has multiple constructors marked with [DynamoMapperConstructor]. Only one constructor can be marked with this attribute.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor CycleDetectedInNestedType = new(
        "DM0006",
        "Circular reference detected in nested type",
        "The property '{0}' creates a circular reference with type '{1}'. Cycles are not supported in nested object mapping.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor UnsupportedNestedMemberType = new(
        "DM0007",
        "Unsupported nested member type",
        "The nested property '{0}.{1}' has type '{2}' which cannot be mapped",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor InvalidDotNotationPath = new(
        "DM0008",
        "Invalid dot-notation path",
        "The dot-notation path '{0}' is invalid. Property '{1}' not found on type '{2}'.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );
}
