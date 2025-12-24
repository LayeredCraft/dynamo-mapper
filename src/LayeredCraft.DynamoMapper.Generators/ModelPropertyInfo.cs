using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal readonly record struct ModelPropertyInfo(string Name, string FullyQualifiedType);

internal static class ModelPropertyInfoExtensions
{
    extension(ModelPropertyInfo)
    {
        internal static ModelPropertyInfo? Create(
            IPropertySymbol propertySymbol,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            if (propertySymbol.IsStatic)
                return null;

            if (propertySymbol.SetMethod is null)
                return null;

            return new ModelPropertyInfo(
                propertySymbol.Name,
                propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
        }
    }
}
