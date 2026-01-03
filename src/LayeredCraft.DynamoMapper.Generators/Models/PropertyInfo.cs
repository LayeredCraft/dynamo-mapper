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
            var name = propertySymbol.Name;
            var key = context.KeyNamingConventionConverter(name);
            var type = propertySymbol.Type as INamedTypeSymbol;
            var isINamedTypeSymbol = type is not null;
            var isNullableType =
                type is { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

            var nullableWord = isNullableType ? "Nullable" : string.Empty;

            var propertyType = isNullableType ? type!.TypeArguments[0] : type;

            return propertyType switch
            {
                // String
                // { SpecialType: SpecialType.System_String } => String(props),

                // Boolean
                { SpecialType: SpecialType.System_Boolean } =>
                    DiagnosticResult<PropertyInfo>.Success(
                        new PropertyInfo(
                            $"""{name} = {fromParamName}.Get{nullableWord}Bool("{key}"),""",
                            $"""item.Set{nullableWord}Bool("{key}", {toParamName}.{name});"""
                        )
                    ),

                _ => DiagnosticResult<PropertyInfo>.Failure(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.CannotConvertFromAttributeValue,
                        propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                        name,
                        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    )
                ),
            };
        }
    }
}
