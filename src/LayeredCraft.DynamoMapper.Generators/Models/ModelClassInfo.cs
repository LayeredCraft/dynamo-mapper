using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal readonly record struct ModelClassInfo(
    string FullyQualifiedType,
    EquatableArray<ModelPropertyInfo> Properties
);

internal static class ModelClassInfoExtensions
{
    extension(ModelClassInfo)
    {
        internal static ModelClassInfo Create(ITypeSymbol modelTypeSymbol, GeneratorContext context)
        {
            context.ThrowIfCancellationRequested();

            var properties = modelTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol => ModelPropertyInfo.Create(propertySymbol, context))
                .WhereNotNull()
                .ToEquatableArray();

            return new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                properties
            );
        }
    }
}
