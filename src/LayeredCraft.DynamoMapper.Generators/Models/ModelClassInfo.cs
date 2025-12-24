using DynamoMapper.Models;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;
using WellKnownType = DynamoMapper.Generator.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator;

internal readonly record struct ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<string> PropertieAssignments
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

            var results = modelTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol =>
                {
                    context.ThrowIfCancellationRequested();

                    if (propertySymbol.IsStatic)
                        return null;

                    if (propertySymbol.SetMethod is null)
                        return null;

                    return BuildFromItemMapping(propertySymbol, context);
                })
                .WhereNotNull()
                .ToLookup(result => result.IsSuccess);

            var successfulMappings = results[true]
                .Select(result => result.Value!)
                .ToEquatableArray();

            var diagnostics = results[false]
                .Select(result => result.Error)
                .OfType<DiagnosticInfo>()
                .ToArray();

            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                successfulMappings
            );

            return (modelClassInfo, diagnostics);
        }
    }

    private static DiagnosticResult<string> BuildFromItemMapping(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    )
    {
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
                { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                    $"""item.GetNullableBool("{name}")"""
                ),
                { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                    $"""item.GetNullableInt("{name}")"""
                ),
                { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                    $"""item.GetNullableLong("{name}")"""
                ),
                { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                    $"""item.GetNullableDouble("{name}")"""
                ),
                { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                    $"""item.GetNullableDecimal("{name}")"""
                ),
                { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                    $"""item.GetNullableDateTime("{name}")"""
                ),
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    DiagnosticResult<string>.Success(
                        $"""item.GetNullableDateTimeOffset("{name}")"""
                    ),
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                    DiagnosticResult<string>.Success($"""item.GetNullableGuid("{name}")"""),
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    DiagnosticResult<string>.Success($"""item.GetNullableTimeSpan("{name}")"""),
                _ => DiagnosticResult<string>.Failure(
                    new DiagnosticInfo(
                        Diagnostics.CannotConvertFromAttributeValue,
                        propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
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
                $"""item.GetString("{name}")"""
            ),

            // Boolean
            { SpecialType: SpecialType.System_Boolean } => DiagnosticResult<string>.Success(
                $"""item.GetBool("{name}")"""
            ),

            // Integer types
            { SpecialType: SpecialType.System_Int32 } => DiagnosticResult<string>.Success(
                $"""item.GetInt("{name}")"""
            ),
            { SpecialType: SpecialType.System_Int64 } => DiagnosticResult<string>.Success(
                $"""item.GetLong("{name}")"""
            ),

            // Floating point types
            { SpecialType: SpecialType.System_Double } => DiagnosticResult<string>.Success(
                $"""item.GetDouble("{name}")"""
            ),
            { SpecialType: SpecialType.System_Decimal } => DiagnosticResult<string>.Success(
                $"""item.GetDecimal("{name}")"""
            ),

            // DateTime
            { SpecialType: SpecialType.System_DateTime } => DiagnosticResult<string>.Success(
                $"""item.GetDateTime("{name}")"""
            ),

            // DateTimeOffset
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                DiagnosticResult<string>.Success($"""item.GetDateTimeOffset("{name}")"""),

            // Guid
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                DiagnosticResult<string>.Success($"""item.GetGuid("{name}")"""),

            // TimeSpan
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                DiagnosticResult<string>.Success($"""item.GetTimeSpan("{name}")"""),

            // Enums
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType =>
                DiagnosticResult<string>.Success(
                    $"""item.GetEnum("{name}", {enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{enumType.MemberNames.FirstOrDefault() ?? "Default"})"""
                ),

            _ => DiagnosticResult<string>.Failure(
                new DiagnosticInfo(
                    Diagnostics.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                )
            ),
        };
    }
}
