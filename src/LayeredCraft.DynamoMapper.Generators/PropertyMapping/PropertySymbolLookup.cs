using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping;

internal static class PropertySymbolLookup
{
    internal static IPropertySymbol[] GetProperties(
        INamedTypeSymbol type,
        bool includeBaseTypes,
        Func<IPropertySymbol, INamedTypeSymbol, bool> predicate
    )
    {
        if (!includeBaseTypes)
        {
            return type
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => predicate(p, type))
                .ToArray();
        }

        var seenNames = new HashSet<string>(StringComparer.Ordinal);
        var properties = new List<IPropertySymbol>();

        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!predicate(property, current))
                    continue;

                // Derived wins by name - skip base properties with the same name
                if (!seenNames.Add(property.Name))
                    continue;

                properties.Add(property);
            }
        }

        return properties.ToArray();
    }

    internal static IPropertySymbol? FindPropertyByName(
        ITypeSymbol type,
        string name,
        bool includeBaseTypes
    )
    {
        if (type is not INamedTypeSymbol namedType)
            return null;

        if (!includeBaseTypes)
        {
            return namedType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name == name);
        }

        for (var current = namedType; current is not null; current = current.BaseType)
        {
            var property = current
                .GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name == name);

            if (property is not null)
                return property;
        }

        return null;
    }
}
