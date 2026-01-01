using DynamoMapper.Runtime;

namespace DynamoMapper.Generator;

internal class MapperOptions
{
    internal DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;
    internal Requiredness DefaultRequiredness { get; set; } = Requiredness.InferFromNullability;
    internal bool OmitNullStrings { get; set; } = true;
    internal bool OmitEmptyStrings { get; set; } = false;
    internal string DateTimeFormat { get; set; } = "O";
    internal EnumFormat EnumFormat { get; set; } = EnumFormat.Name;
}
