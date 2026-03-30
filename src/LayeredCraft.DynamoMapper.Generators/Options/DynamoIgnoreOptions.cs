using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Generator;

internal class DynamoIgnoreOptions
{
    public string MemberName { get; set; } = string.Empty;
    public IgnoreMapping Ignore { get; set; } = IgnoreMapping.All;
}
