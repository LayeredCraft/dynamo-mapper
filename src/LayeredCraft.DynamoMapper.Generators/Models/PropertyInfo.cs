using DynamoMapper.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Props = (string Type, string[] FromArgs, string[] ToArgs);
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
                { SpecialType: SpecialType.System_String } => ("String", [], []),
                { SpecialType: SpecialType.System_Boolean } => ("Bool", [], []),
                { SpecialType: SpecialType.System_Int32 } => ("Int", [], []),
                { SpecialType: SpecialType.System_Int64 } => ("Long", [], []),
                { SpecialType: SpecialType.System_Single } => ("Float", [], []),
                { SpecialType: SpecialType.System_Double } => ("Double", [], []),
                { SpecialType: SpecialType.System_Decimal } => ("Decimal", [], []),
                { SpecialType: SpecialType.System_DateTime } => ("DateTime", [dateFmt], [dateFmt]),
                INamedTypeSymbol t
                    when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) => (
                    "DateTimeOffset",
                    [dateFmt],
                    [dateFmt]
                ),
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) => (
                    "Guid",
                    [],
                    []
                ),
                INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                    ("TimeSpan", [], []),
                INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => (
                    "Enum",
                    [$"{enumType.QualifiedName}.{enumType.MemberNames.First()}"],
                    []
                ),
                _ => null,
            };

            return props is { } p
                ? DiagnosticResult<PropertyInfo>.Success(
                    new PropertyInfo(
                        $"{propertyName} = {fromParamName}.Get{nullable}{p.Type}({baseFromArgs.Concat(p.FromArgs).MakeArgs()}),",
                        $"item.Set{nullable}{p.Type}({baseToArgs.Concat(p.ToArgs).MakeArgs()});"
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
