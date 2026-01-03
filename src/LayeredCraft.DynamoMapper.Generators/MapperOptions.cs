using DynamoMapper.Runtime;
using Humanizer;

namespace DynamoMapper.Generator;

internal class MapperOptions
{
    internal DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;
    internal string DateTimeFormat { get; set; } = "O";
    internal Requiredness DefaultRequiredness { get; set; } = Requiredness.InferFromNullability;
    internal EnumFormat EnumFormat { get; set; } = EnumFormat.Name;
    internal bool OmitEmptyStrings { get; set; } = false;
    internal bool OmitNullStrings { get; set; } = true;

    internal Func<string, string> NameConverter
    {
        get
        {
            field ??= Convention switch
            {
                DynamoNamingConvention.Exact => s => s,
                DynamoNamingConvention.CamelCase => InflectorExtensions.Camelize,
                DynamoNamingConvention.PascalCase => InflectorExtensions.Pascalize,
                DynamoNamingConvention.SnakeCase => InflectorExtensions.Underscore,
                DynamoNamingConvention.KebabCase => InflectorExtensions.Kebaberize,
                _ => throw new InvalidOperationException(
                    $"Unknown {nameof(DynamoNamingConvention)} of {Convention}"
                ),
            };

            return field;
        }
    }
}
