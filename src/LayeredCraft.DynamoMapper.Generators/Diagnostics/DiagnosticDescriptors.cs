using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Diagnostics;

internal static class DiagnosticDescriptors
{
    private const string UsageCategory = "DynamoMapper.Usage";
    private const string GeneratorCategory = "DynamoMapper.Generator";

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

    internal static readonly DiagnosticDescriptor InternalGeneratorError = new(
        "DM0100",
        "Internal generator error",
        "An unexpected error occurred in the DynamoMapper generator: {0}. Please report this issue with the stack trace.",
        GeneratorCategory,
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

    internal static readonly DiagnosticDescriptor TypeResolutionFailure = new(
        "DM0103",
        "Failed to resolve required type",
        "The generator could not resolve the type '{0}'. Ensure all required dependencies are referenced.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor TemplateError = new(
        "DM0104",
        "Template processing error",
        "Failed to load or parse the code generation template: {0}",
        GeneratorCategory,
        DiagnosticSeverity.Error,
        true
    );
}
