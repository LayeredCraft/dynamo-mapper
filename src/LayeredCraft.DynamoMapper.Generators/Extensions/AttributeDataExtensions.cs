using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal static class AttributeDataExtensions
{
    extension(AttributeData attributeData)
    {
        internal TOptions PopulateOptions<TOptions>()
            where TOptions : class, new()
        {
            var options = new TOptions();

            var settingsType = typeof(TOptions);

            // Map named arguments (properties)
            foreach (var (propertyName, value) in attributeData.NamedArguments)
            {
                var property = settingsType.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (property is not null && property.CanWrite)
                {
                    var actualValue = GetTypedConstantValue(value);
                    property.SetValue(options, actualValue);
                }
            }

            return options;
        }
    }

    private static object? GetTypedConstantValue(TypedConstant constant) =>
        constant.Kind switch
        {
            TypedConstantKind.Array => constant.Values.Select(GetTypedConstantValue).ToArray(),
            TypedConstantKind.Type => constant.Value as ITypeSymbol,
            TypedConstantKind.Primitive => constant.Value,
            TypedConstantKind.Enum => GetValidatedEnumValue(constant),
            _ => throw new InvalidOperationException(
                $"TypedConstant of type '{constant.Kind}' is not supported"
            ),
        };

    private static object GetValidatedEnumValue(TypedConstant constant)
    {
        if (constant.Type is not INamedTypeSymbol enumType)
            throw new InvalidOperationException("Enum TypedConstant must have a named type");

        var value = constant.Value;
        if (value == null)
            throw new InvalidOperationException("Enum value cannot be null");

        // Get all declared enum field values
        var validValues = enumType
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsConst && f.HasConstantValue)
            .Select(f => f.ConstantValue)
            .ToHashSet();

        if (validValues.Count == 0)
            throw new InvalidOperationException(
                $"Enum type '{enumType.Name}' has no defined values"
            );

        // Check if it's a Flags enum
        var isFlagsEnum = enumType
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");

        if (isFlagsEnum)
        {
            // For Flags enums, validate bit combinations
            var numericValue = Convert.ToInt64(value);

            // Calculate bitmask of all valid flags
            var validFlagsMask = validValues
                .Select(v => Convert.ToInt64(v))
                .Aggregate(0L, (acc, v) => acc | v);

            // Check if value contains only valid flag bits
            if ((numericValue & ~validFlagsMask) != 0)
                throw new ArgumentException(
                    $"Enum value {numericValue} contains invalid flags for type '{enumType.Name}'"
                );
        }
        else
        {
            // For regular enums, value must exactly match a defined value
            if (!validValues.Contains(value))
            {
                var validList = string.Join(", ", validValues.Select(v => v?.ToString() ?? "null"));
                throw new ArgumentException(
                    $"Enum value '{value}' is not defined in type '{enumType.Name}'. "
                        + $"Valid values: {validList}"
                );
            }
        }

        return value;
    }
}
