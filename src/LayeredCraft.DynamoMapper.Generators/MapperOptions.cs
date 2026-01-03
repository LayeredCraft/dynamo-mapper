using DynamoMapper.Runtime;

namespace DynamoMapper.Generator;

internal class MapperOptions
{
    internal DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;
    internal string DateTimeFormat { get; set; } = "O";
    internal Requiredness DefaultRequiredness { get; set; } = Requiredness.InferFromNullability;
    internal EnumFormat EnumFormat { get; set; } = EnumFormat.Name;
    internal bool OmitEmptyStrings { get; set; } = false;
    internal bool OmitNullStrings { get; set; } = true;
}
