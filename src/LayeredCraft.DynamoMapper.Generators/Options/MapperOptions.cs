using LayeredCraft.DynamoMapper.Extensions;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Generator;

internal class MapperOptions
{
    internal DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;
    internal string DateTimeFormat { get; set; } = "O";
    internal string TimeSpanFormat { get; set; } = "c";
    internal Requiredness DefaultRequiredness { get; set; } = Requiredness.InferFromNullability;
    internal bool IncludeBaseClassProperties { get; set; } = false;
    internal string EnumFormat { get; set; } = "G";
    internal string GuidFormat { get; set; } = "D";
    internal bool OmitEmptyStrings { get; set; } = false;
    internal bool OmitNullValues { get; set; } = true;
    internal bool OmitNullValuesSpecified { get; set; }
    internal bool OmitNullStrings { get; set; } = true;
    internal bool OmitNullStringsSpecified { get; set; }

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
