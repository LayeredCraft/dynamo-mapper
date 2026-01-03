using DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.Models;

internal sealed record ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<string> PropertiedAssignments,
    EquatableArray<string> ToAttributeAssignments
);

internal static class ModelClassInfoExtensions
{
    private static DiagnosticResult<string> BuildFromItemMapping(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        var type = propertySymbol.Type;
        var name = propertySymbol.Name;
        var key = context.KeyNamingConventionConverter(name);

        // Handle nullable types
        if (
            type is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            } nullableType
        )
        {
            var underlyingType = nullableType.TypeArguments[0];
            return underlyingType switch
            {
                // Nullable Boolean
                { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableBool("{key}"),"""
                ),

                // Nullable Integer types
                { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableInt("{key}"),"""
                ),
                { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableLong("{key}"),"""
                ),

                // Nullable Floating point types
                { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableDouble("{key}"),"""
                ),
                { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableDecimal("{key}"),"""
                ),

                // Nullable DateTime
                { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableDateTime("{key}"),"""
                ),

                // Nullable DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableDateTimeOffset("{key}"),"""
                    ),

                // Nullable Guid
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableGuid("{key}"),"""
                    ),

                // Nullable TimeSpan
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableTimeSpan("{key}"),"""
                    ),

                // Nullable Enums
                INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetEnum("{key}", {enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{enumType.MemberNames.FirstOrDefault() ?? "Default"}),"""
                    ),

                _ => DiagnosticResult<string>.Failure(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.CannotConvertFromAttributeValue,
                        propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                        name,
                        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    )
                ),
            };
        }

        // Handle non-nullable types
        return type switch
        {
            // String
            { SpecialType: SpecialType.System_String } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetString("{key}"),"""
            ),

            // Boolean
            { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetBool("{key}"),"""
            ),

            // Integer types
            { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetInt("{key}"),"""
            ),
            { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetLong("{key}"),"""
            ),

            // Floating point types
            { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetDouble("{key}"),"""
            ),
            { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetDecimal("{key}"),"""
            ),

            // DateTime
            { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetDateTime("{key}"),"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success($"""{name} = item.GetDateTimeOffset("{key}"),"""),

            // Guid
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                DiagnosticResult<string>.Success($"""{name} = item.GetGuid("{key}"),"""),

            // TimeSpan
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                DiagnosticResult<string>.Success($"""{name} = item.GetTimeSpan("{key}"),"""),

            // Enums
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType =>
                DiagnosticResult<string>.Success(
                    $"""{name} = item.GetEnum("{key}", {enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{enumType.MemberNames.FirstOrDefault() ?? "Default"}),"""
                ),

            _ => DiagnosticResult<string>.Failure(
                new DiagnosticInfo(
                    DiagnosticDescriptors.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    name,
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
            ),
        };
    }

    private static DiagnosticResult<string> BuildToItemMapping(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        var type = propertySymbol.Type;
        var name = propertySymbol.Name;
        var key = context.KeyNamingConventionConverter(name);

        // Handle nullable types
        if (
            type is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            } nullableType
        )
        {
            var underlyingType = nullableType.TypeArguments[0];
            return underlyingType switch
            {
                // Nullable Boolean
                { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable Integer types
                { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable Floating point types
                { SpecialType: SpecialType.System_Single } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable DateTime
                { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                    ),

                // Nullable Guid
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                    ),

                // Nullable TimeSpan
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                    ),

                // Nullable Enums - convert to string
                INamedTypeSymbol { TypeKind: TypeKind.Enum } => DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                _ => DiagnosticResult<string>.Failure(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.CannotConvertFromAttributeValue,
                        propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                        name,
                        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    )
                ),
            };
        }

        // Handle non-nullable types
        return type switch
        {
            // String - use nullable version since strings are reference types
            { SpecialType: SpecialType.System_String } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue() },"""
            ),

            // Boolean
            { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // Integer types
            { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),
            { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // Floating point types
            { SpecialType: SpecialType.System_Single } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),
            { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),
            { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // DateTime
            { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
                ),

            // Guid
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
                ),

            // TimeSpan
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
                ),

            // Enums - convert to string
            INamedTypeSymbol { TypeKind: TypeKind.Enum } => DiagnosticResult<string>.Success(
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue() },"""
            ),

            _ => DiagnosticResult<string>.Failure(
                new DiagnosticInfo(
                    DiagnosticDescriptors.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    name,
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
            ),
        };
    }

    extension(ModelClassInfo)
    {
        internal static (ModelClassInfo?, DiagnosticInfo[]) Create(
            ITypeSymbol modelTypeSymbol,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            var properties = modelTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod is not null && !p.IsStatic)
                .ToList();

            var (successfulFromMappings, fromDiagnostics) = properties
                .Select(propertySymbol => BuildFromItemMapping(propertySymbol, context))
                .Aggregate(
                    (Successes: new List<string>(), Diagnostics: new List<DiagnosticInfo>()),
                    static (acc, result) =>
                        result.Match(
                            value =>
                            {
                                acc.Successes.Add(value);
                                return acc;
                            },
                            error =>
                            {
                                acc.Diagnostics.Add(error!);
                                return acc;
                            }
                        ),
                    static acc => (acc.Successes.ToEquatableArray(), acc.Diagnostics.ToArray())
                );

            var (successfulToMappings, toDiagnostics) = properties
                .Select(propertySymbol => BuildToItemMapping(propertySymbol, context))
                .Aggregate(
                    (Successes: new List<string>(), Diagnostics: new List<DiagnosticInfo>()),
                    static (acc, result) =>
                        result.Match(
                            value =>
                            {
                                acc.Successes.Add(value);
                                return acc;
                            },
                            error =>
                            {
                                acc.Diagnostics.Add(error!);
                                return acc;
                            }
                        ),
                    static acc => (acc.Successes.ToEquatableArray(), acc.Diagnostics.ToArray())
                );

            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                successfulFromMappings,
                successfulToMappings
            );

            var allDiagnostics = fromDiagnostics.Concat(toDiagnostics).ToArray();
            return (modelClassInfo, allDiagnostics);
        }
    }
}
