using DynamoMapper.Runtime;

namespace DynamoMapper.Generator;

internal class DynamoFieldOptions
{
    public string MemberName { get; set; } = string.Empty;
    public string? AttributeName { get; set; }
    public bool? Required { get; set; }
    public DynamoKind? Kind { get; set; }
    public bool? OmitIfNull { get; set; }
    public bool? OmitIfNullOrWhiteSpace { get; set; }
    public string? ConverterName { get; set; }
    public string? ToMethod { get; set; }
    public string? FromMethod { get; set; }
}
