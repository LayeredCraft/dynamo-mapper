using DynamoMapper.Models;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;
using WellKnownType = DynamoMapper.Generator.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator;

internal readonly record struct ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<string> PropertiedAssignments
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

            var (successfulMappings, diagnostics) = modelTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol =>
                    propertySymbol.SetMethod is null || propertySymbol.IsStatic
                        ? null
                        : BuildFromItemMapping(propertySymbol, context)
                )
                .WhereNotNull()
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
                    $"""{name} = item.GetEnum("{name}", {enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{enumType.MemberNames.FirstOrDefault() ?? "Default"});"""
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
