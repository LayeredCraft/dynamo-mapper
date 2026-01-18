using DynamoMapper.Generator.ConstructorMapping;
using DynamoMapper.Generator.ConstructorMapping.Models;
using DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record ModelClassInfo(
    string FullyQualifiedType,
    string VarName,
    EquatableArray<PropertyInfo> Properties,
    ConstructorInfo? Constructor
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
                .ToArray();

            var varName = context
                .MapperOptions.KeyNamingConventionConverter(modelTypeSymbol.Name)
                .Map(name => name == fromItemParameterName ? name + "1" : name);

            // Phase 1: Constructor Selection (only for FromItem methods)
            var constructorSelectionResult = context.HasFromItemMethod
                ? ConstructorSelector.Select(modelTypeSymbol, properties, context)
                : DiagnosticResult<ConstructorSelectionResult?>.Success(null);

            if (!constructorSelectionResult.IsSuccess)
                return (null, [constructorSelectionResult.Error!]);

            var selectedConstructor = constructorSelectionResult.Value;

            // Phase 2A: Analyze properties with InitializationMethod
            var propertyDiagnosticsList = new List<DiagnosticInfo>();
            var propertyInfosList = new List<PropertyInfo>();

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                // Determine initialization method for this property
                var initMethod =
                    selectedConstructor
                        ?.PropertyModes.FirstOrDefault(pm => pm.PropertyName == property.Name)
                        ?.Method
                    ?? InitializationMethod.InitSyntax;

                var propertyInfoResult = PropertyInfo.Create(
                    property,
                    varName,
                    i,
                    initMethod,
                    context
                );

                if (!propertyInfoResult.IsSuccess)
                {
                    propertyDiagnosticsList.Add(propertyInfoResult.Error!);
                }
                else
                {
                    propertyInfosList.Add(propertyInfoResult.Value!);
                }
            }

            // Phase 2B: Build constructor parameter info (if constructor selected)
            ConstructorInfo? constructorInfo = null;
            if (selectedConstructor is not null)
            {
                var parameterInfosList = new List<ConstructorParameterInfo>();

                foreach (var paramAnalysis in selectedConstructor.Constructor.Parameters)
                {
                    // Find the matching property's PropertyInfo by property name
                    var matchingProperty = paramAnalysis.MatchedProperty;
                    if (matchingProperty is null)
                        continue;

                    // Find the property index in the properties array
                    var propertyIndex = Array.FindIndex(
                        properties,
                        p => SymbolEqualityComparer.Default.Equals(p, matchingProperty)
                    );

                    if (propertyIndex >= 0 && propertyIndex < propertyInfosList.Count)
                    {
                        var matchingPropertyInfo = propertyInfosList[propertyIndex];

                        if (matchingPropertyInfo.FromConstructorArgument is not null)
                        {
                            parameterInfosList.Add(
                                new ConstructorParameterInfo(
                                    paramAnalysis.MemberInfo.MemberName,
                                    matchingPropertyInfo.FromConstructorArgument
                                )
                            );
                        }
                    }
                }

                constructorInfo = new ConstructorInfo(
                    new EquatableArray<ConstructorParameterInfo>(parameterInfosList.ToArray())
                );
            }

            // Phase 3: Create ModelClassInfo with constructor info
            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                varName,
                new EquatableArray<PropertyInfo>(propertyInfosList.ToArray()),
                constructorInfo
            );

            return (modelClassInfo, propertyDiagnosticsList.ToArray());
        }
    }
}
