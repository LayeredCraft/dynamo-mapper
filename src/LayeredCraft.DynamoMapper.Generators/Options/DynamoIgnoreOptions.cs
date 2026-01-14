using DynamoMapper.Runtime;

namespace DynamoMapper.Generator;

internal class DynamoIgnoreOptions
{
    public string MemberName { get; set; } = string.Empty;
    public IgnoreMapping Ignore { get; set; } = IgnoreMapping.All;
}
