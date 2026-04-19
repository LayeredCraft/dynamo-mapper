using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

#if NET8_0_OR_GREATER

public sealed class DateOnlyTimeOnlyModel
{
    public DateOnly StartDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateOnly? OptionalDate { get; set; }
    public TimeOnly? OptionalTime { get; set; }
    public List<DateOnly> AvailableDates { get; set; } = [];
    public List<TimeOnly> AvailableTimes { get; set; } = [];
}

[DynamoMapper]
public static partial class DateOnlyTimeOnlyModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(DateOnlyTimeOnlyModel source);
    public static partial DateOnlyTimeOnlyModel FromItem(Dictionary<string, AttributeValue> item);
}

public sealed class DateOnlyTimeOnlyCustomFormatModel
{
    public DateOnly StartDate { get; set; }
    public TimeOnly StartTime { get; set; }
}

[DynamoMapper(DateOnlyFormat = "yyyyMMdd", TimeOnlyFormat = "HHmm")]
public static partial class DateOnlyTimeOnlyCustomFormatModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(
        DateOnlyTimeOnlyCustomFormatModel source
    );

    public static partial DateOnlyTimeOnlyCustomFormatModel FromItem(
        Dictionary<string, AttributeValue> item
    );
}

#endif
