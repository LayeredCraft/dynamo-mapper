using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class AttributeValueConverterExtensionsTests
{
    [Fact]
    public void String_ToAttributeValue_ReturnsStringAttribute()
    {
        var attribute = "hello".ToAttributeValue();

        attribute.Should().BeEquivalentTo(StringAttribute("hello"));
    }

    [Fact]
    public void NullableString_ToAttributeValue_ReturnsNullAttribute_WhenValueIsNull()
    {
        string? value = null;

        var attribute = value.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Bool_ToAttributeValue_ReturnsBoolAttribute()
    {
        var attribute = true.ToAttributeValue();

        attribute.Should().BeEquivalentTo(BoolAttribute(true));
    }

    [Fact]
    public void NullableBool_ToAttributeValue_ReturnsBoolOrNullAttribute()
    {
        bool? value = false;
        bool? nullValue = null;

        var attribute = value.ToAttributeValue();
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(BoolAttribute(false));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Int_ToAttributeValue_UsesInvariantCultureAndFormatString()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 12345.ToAttributeValue("N0");

        attribute.Should().BeEquivalentTo(NumberAttribute("12,345"));
    }

    [Fact]
    public void NullableInt_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        int? value = 255;
        int? nullValue = null;

        var attribute = value.ToAttributeValue("X");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("FF"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Long_ToAttributeValue_UsesInvariantCultureAndFormatString()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 123456789L.ToAttributeValue("N0");

        attribute.Should().BeEquivalentTo(NumberAttribute("123,456,789"));
    }

    [Fact]
    public void NullableLong_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        long? value = 4095;
        long? nullValue = null;

        var attribute = value.ToAttributeValue("X");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("FFF"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Float_ToAttributeValue_UsesInvariantCulture()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 12.5f.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("12.5"));
    }

    [Fact]
    public void NullableFloat_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        float? value = 12.5f;
        float? nullValue = null;

        var attribute = value.ToAttributeValue("0.00");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("12.50"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Double_ToAttributeValue_UsesInvariantCulture()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 1234.5d.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("1234.5"));
    }

    [Fact]
    public void NullableDouble_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        double? value = 1234.5d;
        double? nullValue = null;

        var attribute = value.ToAttributeValue("0.000");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("1234.500"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Decimal_ToAttributeValue_UsesInvariantCulture()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 1234.5m.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("1234.5"));
    }

    [Fact]
    public void NullableDecimal_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        decimal? value = 1234.5m;
        decimal? nullValue = null;

        var attribute = value.ToAttributeValue("0.000");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(NumberAttribute("1234.500"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void Guid_ToAttributeValue_UsesDefaultDFormat()
    {
        var value = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");

        var attribute = value.ToAttributeValue();

        attribute.Should().BeEquivalentTo(StringAttribute("00112233-4455-6677-8899-aabbccddeeff"));
    }

    [Fact]
    public void NullableGuid_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        Guid? value = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
        Guid? nullValue = null;

        var attribute = value.ToAttributeValue("N");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(StringAttribute("00112233445566778899aabbccddeeff"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void DateTime_ToAttributeValue_UsesDefaultRoundTripFormat()
    {
        var value = new DateTime(2025, 4, 7, 12, 34, 56, DateTimeKind.Utc);

        var attribute = value.ToAttributeValue();

        attribute
            .Should()
            .BeEquivalentTo(StringAttribute(value.ToString("o", CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void NullableDateTime_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        DateTime? value = new DateTime(2025, 4, 7, 12, 34, 56, DateTimeKind.Utc);
        DateTime? nullValue = null;

        var attribute = value.ToAttributeValue("yyyyMMdd");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(StringAttribute("20250407"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void DateTimeOffset_ToAttributeValue_UsesDefaultRoundTripFormat()
    {
        var value = new DateTimeOffset(2025, 4, 7, 12, 34, 56, TimeSpan.FromHours(2));

        var attribute = value.ToAttributeValue();

        attribute
            .Should()
            .BeEquivalentTo(StringAttribute(value.ToString("o", CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void NullableDateTimeOffset_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        DateTimeOffset? value = new DateTimeOffset(2025, 4, 7, 12, 34, 56, TimeSpan.Zero);
        DateTimeOffset? nullValue = null;

        var attribute = value.ToAttributeValue("yyyy-MM-dd zzz");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.Should().BeEquivalentTo(StringAttribute("2025-04-07 +00:00"));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    [Fact]
    public void TimeSpan_ToAttributeValue_UsesDefaultConstantFormat()
    {
        var value = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3);

        var attribute = value.ToAttributeValue();

        attribute
            .Should()
            .BeEquivalentTo(StringAttribute(value.ToString("c", CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void NullableTimeSpan_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        TimeSpan? value =
            TimeSpan.FromHours(26) + TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(4);
        TimeSpan? nullValue = null;

        var attribute = value.ToAttributeValue("g");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute
            .Should()
            .BeEquivalentTo(
                StringAttribute(value.Value.ToString("g", CultureInfo.InvariantCulture)));
        nullAttribute.Should().BeEquivalentTo(NullAttribute());
    }

    private static AttributeValue StringAttribute(string value) => new() { S = value };

    private static AttributeValue NumberAttribute(string value) => new() { N = value };

    private static AttributeValue BoolAttribute(bool value) => new() { BOOL = value };

    private static AttributeValue NullAttribute() => new() { NULL = true };

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string name)
        {
            var culture = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
