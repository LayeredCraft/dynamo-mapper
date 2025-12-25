using DynamoMapper.Models;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;
using WellKnownType = DynamoMapper.Generator.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator;

internal readonly record struct ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<string> PropertiedAssignments,
    EquatableArray<string> ToAttributeAssignments
);

internal static class ModelClassInfoExtensions
{
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
                                acc.Diagnostics.Add(error!.Value);
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
                                acc.Diagnostics.Add(error!.Value);
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

    private static DiagnosticResult<string> BuildFromItemMapping(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        var type = propertySymbol.Type;
        var name = propertySymbol.Name;

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
                    $"""{name} = item.GetNullableBool("{name}"),"""
                ),

                // Nullable Integer types
                { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableInt("{name}"),"""
                ),
                { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableLong("{name}"),"""
                ),

                // Nullable Floating point types
                { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableDouble("{name}"),"""
                ),
                { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableDecimal("{name}"),"""
                ),

                // Nullable DateTime
                { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                    $"""{name} = item.GetNullableDateTime("{name}"),"""
                ),

                // Nullable DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableDateTimeOffset("{name}"),"""
                    ),

                // Nullable Guid
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableGuid("{name}"),"""
                    ),

                // Nullable TimeSpan
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableTimeSpan("{name}"),"""
                    ),

                // Nullable Enums
                INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetEnum("{name}", {enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{enumType.MemberNames.FirstOrDefault() ?? "Default"}),"""
                    ),

                _ => DiagnosticResult<string>.Failure(
                    new DiagnosticInfo(
                        Diagnostics.CannotConvertFromAttributeValue,
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
                $"""{name} = item.GetString("{name}"),"""
            ),

            // Boolean
            { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetBool("{name}"),"""
            ),

            // Integer types
            { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetInt("{name}"),"""
            ),
            { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetLong("{name}"),"""
            ),

            // Floating point types
            { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetDouble("{name}"),"""
            ),
            { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetDecimal("{name}"),"""
            ),

            // DateTime
            { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                $"""{name} = item.GetDateTime("{name}"),"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success($"""{name} = item.GetDateTimeOffset("{name}"),"""),

            // Guid
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                DiagnosticResult<string>.Success($"""{name} = item.GetGuid("{name}"),"""),

            // TimeSpan
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                DiagnosticResult<string>.Success($"""{name} = item.GetTimeSpan("{name}"),"""),

            // Enums
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType =>
                DiagnosticResult<string>.Success(
                    $"""{name} = item.GetEnum("{name}", {enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{enumType.MemberNames.FirstOrDefault() ?? "Default"}),"""
                ),

            _ => DiagnosticResult<string>.Failure(
                new DiagnosticInfo(
                    Diagnostics.CannotConvertFromAttributeValue,
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
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable Integer types
                { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable Floating point types
                { SpecialType: SpecialType.System_Single } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable DateTime
                { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                // Nullable DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                    ),

                // Nullable Guid
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                    ),

                // Nullable TimeSpan
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                    ),

                // Nullable Enums - convert to string
                INamedTypeSymbol { TypeKind: TypeKind.Enum } => DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
                ),

                _ => DiagnosticResult<string>.Failure(
                    new DiagnosticInfo(
                        Diagnostics.CannotConvertFromAttributeValue,
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
                $$"""{ "{{name}}", source.{{name}}.ToNullableAttributeValue() },"""
            ),

            // Boolean
            { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // Integer types
            { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),
            { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // Floating point types
            { SpecialType: SpecialType.System_Single } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),
            { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),
            { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // DateTime
            { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
                ),

            // Guid
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
                ),

            // TimeSpan
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
                ),

            // Enums - convert to string
            INamedTypeSymbol { TypeKind: TypeKind.Enum } => DiagnosticResult<string>.Success(
                $$"""{ "{{name}}", source.{{name}}.ToAttributeValue() },"""
            ),

            _ => DiagnosticResult<string>.Failure(
                new DiagnosticInfo(
                    Diagnostics.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    name,
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
            ),
        };
    }
}
