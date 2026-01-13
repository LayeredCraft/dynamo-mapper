using DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<PropertyInfo> Properties
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
                .Where(p => !p.IsStatic)
                .ToList();

            var (propertyInfos, propertyDiagnostics) = properties.CollectDiagnosticResults(
                propertySymbol => PropertyInfo.Create(propertySymbol, context)
            );

            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                propertyInfos.ToEquatableArray()
            );

            return (modelClassInfo, propertyDiagnostics.ToArray());
        }
    }
}
