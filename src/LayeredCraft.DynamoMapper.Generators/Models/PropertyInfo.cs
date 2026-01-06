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
            var isNullableType =
                propertySymbol.Type as INamedTypeSymbol
                is { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T }
                    or { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated };

            return GetProps(propertySymbol, context, isNullableType)
                .Bind(p =>
                {
                    var toParamName = context.MapperOptions.ToMethodParameterName;
                    var fromParamName = context.MapperOptions.FromMethodParameterName;
                    var propertyName = propertySymbol.Name;
                    var key = context
                        .MapperOptions.KeyNamingConventionConverter(propertyName)
                        .Map(k => $"\"{k}\"");
                    var sourceProperty = $"{toParamName}.{propertyName}";

                    var nullable = isNullableType ? "Nullable" : string.Empty;

                    var fromArgs = (
                        (string[])
                            [
                                key,
                                .. p.FromArgs,
                                $"Requiredness.{context.MapperOptions.DefaultRequiredness.ToString()}",
                            ]
                    ).MakeArgs();

                    var toArgs = (
                        (string[])
                            [
                                key,
                                sourceProperty,
                                .. p.ToArgs,
                                $"{context.MapperOptions.OmitEmptyStrings.ToString().ToLowerInvariant()}",
                                $"{context.MapperOptions.OmitNullStrings.ToString().ToLowerInvariant()}",
                            ]
                    ).MakeArgs();

                    return DiagnosticResult<PropertyInfo>.Success(
                        new PropertyInfo(
                            $"{propertyName} = {fromParamName}.Get{nullable}{p.Type}{p.Generic}({fromArgs}),",
                            $"item.Set{p.Type}{p.Generic}({toArgs});"
                        )
                    );
                });
        }
    }

    private static DiagnosticResult<Props> GetProps(
        IPropertySymbol propertySymbol,
        GeneratorContext context,
        bool isNullableType
    ) =>
        (
            propertySymbol.Type
                is INamedTypeSymbol
                {
                    OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
                } namedType
                ? namedType.TypeArguments[0]
                : propertySymbol.Type
        ) switch
        {
            { SpecialType: SpecialType.System_String } => ("String", [], [], string.Empty),
            { SpecialType: SpecialType.System_Boolean } => ("Bool", [], [], string.Empty),
            { SpecialType: SpecialType.System_Int32 } => ("Int", [], [], string.Empty),
            { SpecialType: SpecialType.System_Int64 } => ("Long", [], [], string.Empty),
            { SpecialType: SpecialType.System_Single } => ("Float", [], [], string.Empty),
            { SpecialType: SpecialType.System_Double } => ("Double", [], [], string.Empty),
            { SpecialType: SpecialType.System_Decimal } => ("Decimal", [], [], string.Empty),
            { SpecialType: SpecialType.System_DateTime } =>
                $"\"{context.MapperOptions.DateTimeFormat}\"".Map(
                    static Props (dateFmt) => ("DateTime", [dateFmt], [dateFmt], string.Empty)
                ),
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                $"\"{context.MapperOptions.DateTimeFormat}\"".Map(
                    static Props (dateFmt) => ("DateTimeOffset", [dateFmt], [dateFmt], string.Empty)
                ),
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) => (
                "Guid",
                [],
                [],
                string.Empty
            ),
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) => (
                "TimeSpan",
                [],
                [],
                string.Empty
            ),
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => enumType.QualifiedName.Map(
                Props (name) =>
                    (
                        "Enum",
                        isNullableType ? [] : [$"{name}.{enumType.MemberNames.First()}"],
                        [],
                        $"<{name}>"
                    )
            ),
            _ => DiagnosticResult<Props>.Failure(
                DiagnosticDescriptors.CannotConvertFromAttributeValue,
                propertySymbol.Locations.FirstOrDefault()?.CreateLocationInfo(),
                propertySymbol.Name,
                propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            ),
        };

    extension(IEnumerable<string> strings)
    {
        private string MakeArgs() => string.Join(", ", strings);
    }
}
