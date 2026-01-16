using DynamoMapper.Extensions;
using DynamoMapper.Runtime;

namespace DynamoMapper.Generator;

internal class MapperOptions
{
    internal DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;
    internal string DateTimeFormat { get; set; } = "O";
    internal string TimeSpanFormat { get; set; } = "c";
    internal Requiredness DefaultRequiredness { get; set; } = Requiredness.InferFromNullability;
    internal string EnumFormat { get; set; } = "G";
    internal bool OmitEmptyStrings { get; set; } = false;
    internal bool OmitNullStrings { get; set; } = true;

    internal string ToMethodParameterName { get; set; } = "source";
    internal string FromMethodParameterName { get; set; } = "item";

    internal Func<string, string> KeyNamingConventionConverter
    {
        get
        {
            field ??= Convention.GetConverter();
            return field;
        }
    }
}
