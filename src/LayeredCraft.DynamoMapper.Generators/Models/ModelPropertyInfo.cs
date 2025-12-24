using DynamoMapper.Models;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;
using WellKnownType = DynamoMapper.Generator.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator;

internal readonly record struct ModelPropertyInfo(string Name, string FullyQualifiedType);

internal static class ModelPropertyInfoExtensions
{
    extension(ModelPropertyInfo)
    {
        internal static ModelPropertyInfo? Create(
            IPropertySymbol propertySymbol,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            if (propertySymbol.IsStatic)
                return null;

            if (propertySymbol.SetMethod is null)
                return null;

            var fromItemResult = BuildFromItemMapping(propertySymbol, context);

            return new ModelPropertyInfo(
                propertySymbol.Name,
                propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
        }
    }

    private static DiagnosticResult<string> BuildFromItemMapping(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    ) =>
        (propertySymbol.Type as INamedTypeSymbol) switch
        {
            { } t when t.IsAssignableTo(WellKnownType.System_String, context) =>
                DiagnosticResult<string>.Success($"""item.GetString("{propertySymbol.Name}")"""),

            _ => DiagnosticResult<string>.Failure(
                new DiagnosticInfo(
                    Diagnostics.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo()
                )
            ),
        };
}
