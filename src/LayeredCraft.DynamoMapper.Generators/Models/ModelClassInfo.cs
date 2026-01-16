using DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record ModelClassInfo(
    string FullyQualifiedType,
    string VarName,
    EquatableArray<PropertyInfo> Properties
);

internal static class ModelClassInfoExtensions
{
    extension(ModelClassInfo)
    {
        internal static (ModelClassInfo?, DiagnosticInfo[]) Create(
            ITypeSymbol modelTypeSymbol,
            string? fromItemParameterName,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            var properties = modelTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p =>
                    !p.IsStatic && (!modelTypeSymbol.IsRecord || p.Name != "EqualityContract")
                )
                .ToList();

            var varName = context
                .MapperOptions.KeyNamingConventionConverter(modelTypeSymbol.Name)
                .Map(name => name == fromItemParameterName ? name + "1" : name);

            var (propertyInfos, propertyDiagnostics) = properties.CollectDiagnosticResults(
                propertySymbol => PropertyInfo.Create(propertySymbol, varName, context)
            );

            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                varName,
                propertyInfos.ToEquatableArray()
            );

            return (modelClassInfo, propertyDiagnostics.ToArray());
        }
    }
}
