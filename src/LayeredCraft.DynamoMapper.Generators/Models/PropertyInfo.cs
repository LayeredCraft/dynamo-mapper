using DynamoMapper.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Props = (string Type, string[] FromArgs, string[] ToArgs, string Generic);
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
            context.ThrowIfCancellationRequested();

            var toParamName = context.MapperOptions.ToMethodParameterName;
            var fromParamName = context.MapperOptions.FromMethodParameterName;
            var propertyName = propertySymbol.Name;
            var key = context
                .MapperOptions.KeyNamingConventionConverter(propertyName)
                .Map(k => $"\"{k}\"");
            var sourceProperty = $"{toParamName}.{propertyName}";
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

            string[] baseFromArgs = [key];
            string[] baseToArgs = [key, sourceProperty];

            var dateFmt = $"\"{context.MapperOptions.DateTimeFormat}\"";

            Props? props = propertyType switch
            {
                { SpecialType: SpecialType.System_String } => ("String", [], [], string.Empty),
                { SpecialType: SpecialType.System_Boolean } => ("Bool", [], [], string.Empty),
                { SpecialType: SpecialType.System_Int32 } => ("Int", [], [], string.Empty),
                { SpecialType: SpecialType.System_Int64 } => ("Long", [], [], string.Empty),
                { SpecialType: SpecialType.System_Single } => ("Float", [], [], string.Empty),
                { SpecialType: SpecialType.System_Double } => ("Double", [], [], string.Empty),
                { SpecialType: SpecialType.System_Decimal } => ("Decimal", [], [], string.Empty),
                { SpecialType: SpecialType.System_DateTime } => (
                    "DateTime",
                    [dateFmt],
                    [dateFmt],
                    string.Empty
                ),
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) => (
                    "DateTimeOffset",
                    [dateFmt],
                    [dateFmt],
                    string.Empty
                ),
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) => (
                    "Guid",
                    [],
                    [],
                    string.Empty
                ),
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    ("TimeSpan", [], [], string.Empty),
                INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => enumType.QualifiedName.Map(
                    Props (name) =>
                        (
                            "Enum",
                            isNullableType ? [] : [$"{name}.{enumType.MemberNames.First()}"],
                            [],
                            $"<{name}>"
                        )
                ),
                _ => null,
            };

            var requiredness =
                $"Requiredness.{context.MapperOptions.DefaultRequiredness.ToString()}";
            var omitOptions =
                $"{context.MapperOptions.OmitEmptyStrings.ToString().ToLowerInvariant()}, {context.MapperOptions.OmitNullStrings.ToString().ToLowerInvariant()}";

            return props is { } p
                ? DiagnosticResult<PropertyInfo>.Success(
                    new PropertyInfo(
                        $"{propertyName} = {fromParamName}.Get{nullable}{p.Type}{p.Generic}({baseFromArgs.Concat([.. p.FromArgs, requiredness]).MakeArgs()}),",
                        $"item.Set{p.Type}{p.Generic}({baseToArgs.Concat([.. p.ToArgs, omitOptions]).MakeArgs()});"
                    )
                )
                : DiagnosticResult<PropertyInfo>.Failure(
                    DiagnosticDescriptors.CannotConvertFromAttributeValue,
                    propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    propertyName,
                    type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                );
        }
    }

    extension(IEnumerable<string> strings)
    {
        private string MakeArgs() => string.Join(", ", strings);
    }
}
