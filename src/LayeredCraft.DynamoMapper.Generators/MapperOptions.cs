using DynamoMapper.Runtime;

namespace DynamoMapper.Generator;

internal class MapperOptions
{
    internal DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;
    internal bool DefaultRequiredness { get; set; } = true;
    internal bool OmitNullStrings { get; set; } = true;
    internal bool OmitEmptyStrings { get; set; } = false;
    internal string DateTimeFormat { get; set; } = "O";
    internal string EnumFormat { get; set; } = "Name";
}
