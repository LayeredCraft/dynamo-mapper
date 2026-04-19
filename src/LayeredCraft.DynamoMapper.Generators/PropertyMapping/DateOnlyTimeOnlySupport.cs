using Microsoft.CodeAnalysis;

namespace LayeredCraft.DynamoMapper.Generator.PropertyMapping;

internal static class DateOnlyTimeOnlySupport
{
    private const string DateOnlyMetadataName = "System.DateOnly";
    private const string TimeOnlyMetadataName = "System.TimeOnly";
    private const string RuntimeExtensionsMetadataName =
        "Amazon.DynamoDBv2.Model.DateOnlyTimeOnlyAttributeValueExtensions";

    internal static bool RuntimeApisAvailable(GeneratorContext context) =>
        context.SemanticModel.Compilation.GetTypeByMetadataName(RuntimeExtensionsMetadataName)
        is not null;

    internal static bool IsDateOnly(INamedTypeSymbol type, GeneratorContext context)
    {
        var symbol = context.SemanticModel.Compilation.GetTypeByMetadataName(DateOnlyMetadataName);
        return symbol is not null && SymbolEqualityComparer.Default.Equals(type, symbol);
    }

    internal static bool IsTimeOnly(INamedTypeSymbol type, GeneratorContext context)
    {
        var symbol = context.SemanticModel.Compilation.GetTypeByMetadataName(TimeOnlyMetadataName);
        return symbol is not null && SymbolEqualityComparer.Default.Equals(type, symbol);
    }
}
