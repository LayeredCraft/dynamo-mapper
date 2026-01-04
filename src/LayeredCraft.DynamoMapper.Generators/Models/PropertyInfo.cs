using DynamoMapper.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record PropertyInfo(string FromAssignment, string ToAssignments);

internal static class PropertyInfoExtensions
{
    extension(PropertyInfo)
    {
        internal static DiagnosticResult<PropertyInfo> Create(
            IPropertySymbol propertySymbol,
            GeneratorContext context
        )
        {
            var toParamName = "source";
            var fromParamName = "item";
            var propertyName = propertySymbol.Name;
            var key = context.KeyNamingConventionConverter(propertyName);
            var type = propertySymbol.Type as INamedTypeSymbol;
            var isINamedTypeSymbol = type is not null;
            var isNullableType =
                type is { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

            var nullable = isNullableType ? "Nullable" : string.Empty;

            var propertyType = isNullableType ? type!.TypeArguments[0] : type;

            var propertyInfo = propertyType switch
            {
                // String
                { SpecialType: SpecialType.System_String } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}String("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{fromParamName}}.To{{nullable}}AttributeValue() },"""
                ),

                // Boolean
                // $"""item.Set{nullable}Bool("{key}", {toParamName}.{propertyName});"""
                { SpecialType: SpecialType.System_Boolean } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Bool("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{fromParamName}}.To{{nullable}}AttributeValue() },"""
                ),

                _ => null,
            };

            return propertyInfo is not null
                ? DiagnosticResult<PropertyInfo>.Success(propertyInfo)
                : DiagnosticResult<PropertyInfo>.Failure(
                    DiagnosticDescriptors.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    propertyName,
                    type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                );
        }
    }
}
