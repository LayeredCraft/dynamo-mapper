using DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.Models;

internal sealed record ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<string> PropertiedAssignments,
    EquatableArray<string> ToAttributeAssignments,
    EquatableArray<PropertyInfo> Properties
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
                    $"""{name} = item.GetNullableDateTimeExact("{key}", "{context.MapperOptions.DateTimeFormat}"),"""
                ),

                // Nullable DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $"""{name} = item.GetNullableDateTimeOffsetExact("{key}", "{context.MapperOptions.DateTimeFormat}"),"""
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
                $"""{name} = item.GetDateTimeExact("{key}", "{context.MapperOptions.DateTimeFormat}"),"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success(
                    $"""{name} = item.GetDateTimeOffsetExact("{key}", "{context.MapperOptions.DateTimeFormat}"),"""
                ),

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
                    $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue("{{context.MapperOptions.DateTimeFormat}}") },"""
                ),

                // Nullable DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $$"""{ "{{key}}", source.{{name}}.ToNullableAttributeValue("{{context.MapperOptions.DateTimeFormat}}") },"""
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
                $$"""{ "{{key}}", source.{{name}}.ToAttributeValue("{{context.MapperOptions.DateTimeFormat}}") },"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success(
                    $$"""{ "{{key}}", source.{{name}}.ToAttributeValue("{{context.MapperOptions.DateTimeFormat}}") },"""
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

            var (successfulFromMappings, fromDiagnostics) = properties.CollectDiagnosticResults(
                propertySymbol => BuildFromItemMapping(propertySymbol, context)
            );

            var (successfulToMappings, toDiagnostics) = properties.CollectDiagnosticResults(
                propertySymbol => BuildToItemMapping(propertySymbol, context)
            );

            var (propertyInfos, propertyDiagnostics) = properties.CollectDiagnosticResults(
                propertySymbol => PropertyInfo.Create(propertySymbol, context)
            );

            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                successfulFromMappings.ToEquatableArray(),
                successfulToMappings.ToEquatableArray(),
                propertyInfos.ToEquatableArray()
            );

            var allDiagnostics = fromDiagnostics
                .Concat(toDiagnostics)
                .Concat(propertyDiagnostics)
                .ToArray();
            return (modelClassInfo, allDiagnostics);
        }
    }
}
