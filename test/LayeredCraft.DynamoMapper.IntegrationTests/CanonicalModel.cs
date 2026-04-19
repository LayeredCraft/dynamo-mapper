using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

public enum CanonicalStatus
{
    Draft,
    Active,
    Archived,
}

public sealed class CanonicalNestedDetail
{
    public string Region { get; set; } = string.Empty;

    public DateTimeOffset ReviewedAt { get; set; }

    public bool IsPrimary { get; set; }
}

public sealed class CanonicalChild
{
    public string Name { get; set; } = string.Empty;

    public Guid Identifier { get; set; }

    public DateTime LastSeenAt { get; set; }
}

public sealed class CanonicalBinaryScalarModel
{
    public byte[] Payload { get; set; } = [];
}

public sealed class CanonicalBinaryStreamScalarModel
{
    public Stream Payload { get; set; } = Stream.Null;
}

public sealed class CanonicalIntegrationModel
{
    public string Id { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public int Count { get; set; }

    public long TotalCount { get; set; }

    public float Ratio { get; set; }

    public double Score { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public TimeSpan ProcessingTime { get; set; }

    public Guid CorrelationId { get; set; }

    public CanonicalStatus Status { get; set; }

    public string? OptionalText { get; set; }

    public int? OptionalNumber { get; set; }

    public bool? OptionalFlag { get; set; }

    public string[] Tags { get; set; } = [];

    public List<int> Scores { get; set; } = [];

    public IEnumerable<string> Aliases { get; set; } = [];

    public List<Guid> RelatedIds { get; set; } = [];

    public Guid[] LegacyIds { get; set; } = [];

    public IEnumerable<Guid> AlternateIds { get; set; } = [];

    public HashSet<Guid> UniqueIds { get; set; } = [];

    public Dictionary<string, decimal> PriceByMarket { get; set; } = [];

    public Dictionary<string, Guid> ContactIdsByRole { get; set; } = [];

    public HashSet<string> Labels { get; set; } = [];

    public HashSet<int> ImportanceCodes { get; set; } = [];

    public List<byte[]> PayloadVersions { get; set; } = [];

    public Dictionary<string, byte[]> PayloadByName { get; set; } = [];

    public HashSet<byte[]> UniquePayloads { get; set; } = [];

    public CanonicalNestedDetail Detail { get; set; } = new();

    public List<CanonicalChild> Contacts { get; set; } = [];

    public Dictionary<string, CanonicalChild> ContactsById { get; set; } = [];
}

[DynamoMapper]
public static partial class CanonicalIntegrationModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(
        CanonicalIntegrationModel source
    );

    public static partial CanonicalIntegrationModel FromItem(
        Dictionary<string, AttributeValue> item
    );
}

[DynamoMapper]
public static partial class CanonicalBinaryScalarModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(
        CanonicalBinaryScalarModel source
    );

    public static partial CanonicalBinaryScalarModel FromItem(
        Dictionary<string, AttributeValue> item
    );
}

[DynamoMapper]
public static partial class CanonicalBinaryStreamScalarModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(
        CanonicalBinaryStreamScalarModel source
    );

    public static partial CanonicalBinaryStreamScalarModel FromItem(
        Dictionary<string, AttributeValue> item
    );
}

public enum CanonicalFormattedStatus
{
    Draft = 1,
    Published = 2,
}

public sealed class CanonicalFormattedCollectionModel
{
    public List<Guid> RelatedIds { get; set; } = [];

    public Dictionary<string, TimeSpan> DurationsByName { get; set; } = [];

    public HashSet<CanonicalFormattedStatus> Statuses { get; set; } = [];
}

[DynamoMapper(GuidFormat = "N", TimeSpanFormat = "G", EnumFormat = "D")]
public static partial class CanonicalFormattedCollectionModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(
        CanonicalFormattedCollectionModel source
    );

    public static partial CanonicalFormattedCollectionModel FromItem(
        Dictionary<string, AttributeValue> item
    );
}
