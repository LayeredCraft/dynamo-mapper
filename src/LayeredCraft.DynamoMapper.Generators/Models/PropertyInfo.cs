using DynamoMapper.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

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
            var isNullableType =
                type
                is { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T }
                    or { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated };

            var nullable = isNullableType ? "Nullable" : string.Empty;

            var propertyType = type
                is { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T }
                ? type.TypeArguments[0]
                : propertySymbol.Type;

            var propertyInfo = propertyType switch
            {
                // String
                { SpecialType: SpecialType.System_String } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}String("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),

                // Boolean
                // $"""item.Set{nullable}Bool("{key}", {toParamName}.{propertyName});"""
                { SpecialType: SpecialType.System_Boolean } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Bool("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),

                // Integer types
                { SpecialType: SpecialType.System_Int32 } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Int("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Int64 } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Long("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),

                // Floating point types
                { SpecialType: SpecialType.System_Single } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Float("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Double } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Double("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),
                { SpecialType: SpecialType.System_Decimal } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}Decimal("{key}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                ),

                // DateTime
                { SpecialType: SpecialType.System_DateTime } => new PropertyInfo(
                    $"""{propertyName} = {fromParamName}.Get{nullable}DateTimeExact("{key}", "{context.MapperOptions.DateTimeFormat}"),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue("{{context.MapperOptions.DateTimeFormat}}") },"""
                ),

                // DateTimeOffset
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                    new PropertyInfo(
                        $"""{propertyName} = {fromParamName}.Get{nullable}DateTimeOffsetExact("{key}", "{context.MapperOptions.DateTimeFormat}"),""",
                        $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue("{{context.MapperOptions.DateTimeFormat}}") },"""
                    ),

                // Guid
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                    new PropertyInfo(
                        $"""{propertyName} = {fromParamName}.Get{nullable}Guid("{key}"),""",
                        $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                    ),

                // TimeSpan
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    new PropertyInfo(
                        $"""{propertyName} = {fromParamName}.Get{nullable}TimeSpan("{key}"),""",
                        $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
                    ),

                // Enums
                INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => new PropertyInfo(
                    isNullableType
                        ? $"""{propertyName} = {fromParamName}.Get{nullable}Enum<{enumType.ToNotNullableGloballyQualifiedName()}>("{key}"),"""
                        : $"""{propertyName} = {fromParamName}.Get{nullable}Enum("{key}", {enumType.ToNotNullableGloballyQualifiedName()}.{enumType.MemberNames.First()}),""",
                    $$"""{ "{{key}}", {{toParamName}}.{{propertyName}}.To{{nullable}}AttributeValue() },"""
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
