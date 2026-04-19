using System.Globalization;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

#if NET8_0_OR_GREATER

public class DateOnlyTimeOnlyMapperTests
{
    private static readonly DateOnly SampleDate = new(2024, 6, 15);
    private static readonly TimeOnly SampleTime = new(14, 30, 45, 123, 456);

    [Fact]
    public void DateOnly_RoundTrip_PreservesValue()
    {
        var expected = new DateOnlyTimeOnlyModel { StartDate = SampleDate, StartTime = SampleTime };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.StartDate.Should().Be(expected.StartDate);
    }

    [Fact]
    public void TimeOnly_RoundTrip_PreservesValue()
    {
        var expected = new DateOnlyTimeOnlyModel { StartDate = SampleDate, StartTime = SampleTime };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.StartTime.Should().Be(expected.StartTime);
    }

    [Fact]
    public void DateOnly_ToItem_StoresDefaultIso8601Format()
    {
        var model = new DateOnlyTimeOnlyModel { StartDate = SampleDate, StartTime = SampleTime };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(model);

        item["startDate"].S.Should().Be("2024-06-15");
    }

    [Fact]
    public void TimeOnly_ToItem_StoresDefaultFormat()
    {
        var model = new DateOnlyTimeOnlyModel { StartDate = SampleDate, StartTime = SampleTime };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(model);

        item["startTime"]
            .S.Should()
            .Be(SampleTime.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullableDateOnly_RoundTrip_PreservesNonNullValue()
    {
        var expected =
            new DateOnlyTimeOnlyModel
            {
                StartDate = SampleDate,
                StartTime = SampleTime,
                OptionalDate = new DateOnly(2025, 12, 31),
            };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.OptionalDate.Should().Be(expected.OptionalDate);
    }

    [Fact]
    public void NullableDateOnly_RoundTrip_PreservesNullValue()
    {
        var expected =
            new DateOnlyTimeOnlyModel
            {
                StartDate = SampleDate, StartTime = SampleTime, OptionalDate = null,
            };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.OptionalDate.Should().BeNull();
    }

    [Fact]
    public void NullableTimeOnly_RoundTrip_PreservesNonNullValue()
    {
        var expected =
            new DateOnlyTimeOnlyModel
            {
                StartDate = SampleDate,
                StartTime = SampleTime,
                OptionalTime = new TimeOnly(9, 0, 0),
            };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.OptionalTime.Should().Be(expected.OptionalTime);
    }

    [Fact]
    public void NullableTimeOnly_RoundTrip_PreservesNullValue()
    {
        var expected =
            new DateOnlyTimeOnlyModel
            {
                StartDate = SampleDate, StartTime = SampleTime, OptionalTime = null,
            };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.OptionalTime.Should().BeNull();
    }

    [Fact]
    public void DateOnlyList_RoundTrip_PreservesValues()
    {
        var expected =
            new DateOnlyTimeOnlyModel
            {
                StartDate = SampleDate,
                StartTime = SampleTime,
                AvailableDates =
                [
                    new DateOnly(2024, 1, 1),
                    new DateOnly(2024, 6, 15),
                    new DateOnly(2024, 12, 31),
                ],
            };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.AvailableDates.Should().Equal(expected.AvailableDates);
    }

    [Fact]
    public void TimeOnlyList_RoundTrip_PreservesValues()
    {
        var expected =
            new DateOnlyTimeOnlyModel
            {
                StartDate = SampleDate,
                StartTime = SampleTime,
                AvailableTimes =
                [
                    new TimeOnly(8, 0), new TimeOnly(12, 30), new TimeOnly(17, 45),
                ],
            };

        var item = DateOnlyTimeOnlyModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyModelMapper.FromItem(item);

        actual.AvailableTimes.Should().Equal(expected.AvailableTimes);
    }

    [Fact]
    public void DateOnly_CustomFormat_RoundTrip_PreservesValue()
    {
        var expected =
            new DateOnlyTimeOnlyCustomFormatModel
            {
                StartDate = SampleDate, StartTime = new TimeOnly(14, 30),
            };

        var item = DateOnlyTimeOnlyCustomFormatModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyCustomFormatModelMapper.FromItem(item);

        item["startDate"].S.Should().Be("20240615");
        actual.StartDate.Should().Be(expected.StartDate);
    }

    [Fact]
    public void TimeOnly_CustomFormat_RoundTrip_PreservesValue()
    {
        var expected =
            new DateOnlyTimeOnlyCustomFormatModel
            {
                StartDate = SampleDate, StartTime = new TimeOnly(14, 30),
            };

        var item = DateOnlyTimeOnlyCustomFormatModelMapper.ToItem(expected);
        var actual = DateOnlyTimeOnlyCustomFormatModelMapper.FromItem(item);

        item["startTime"].S.Should().Be("1430");
        actual.StartTime.Should().Be(expected.StartTime);
    }
}

#endif
