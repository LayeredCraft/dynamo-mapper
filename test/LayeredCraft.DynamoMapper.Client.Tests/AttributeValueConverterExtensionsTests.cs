using System.Globalization;
using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class AttributeValueConverterExtensionsTests
{
    [Fact]
    public void String_ToAttributeValue_ReturnsStringAttribute()
    {
        var attribute = "hello".ToAttributeValue();

        attribute.S.Should().Be("hello");
        attribute.NULL.Should().NotBeTrue();
    }

    [Fact]
    public void NullableString_ToAttributeValue_ReturnsNullAttribute_WhenValueIsNull()
    {
        string? value = null;

        var attribute = value.ToAttributeValue();

        AssertNullAttribute(attribute);
    }

    [Fact]
    public void Bool_ToAttributeValue_ReturnsBoolAttribute()
    {
        var attribute = true.ToAttributeValue();

        attribute.BOOL.Should().BeTrue();
        attribute.NULL.Should().NotBeTrue();
    }

    [Fact]
    public void NullableBool_ToAttributeValue_ReturnsBoolOrNullAttribute()
    {
        bool? value = false;
        bool? nullValue = null;

        var attribute = value.ToAttributeValue();
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.BOOL.Should().BeFalse();
        attribute.NULL.Should().NotBeTrue();
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void Int_ToAttributeValue_UsesInvariantCultureAndFormatString()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 12345.ToAttributeValue("N0");

        attribute.N.Should().Be("12,345");
    }

    [Fact]
    public void NullableInt_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        int? value = 255;
        int? nullValue = null;

        var attribute = value.ToAttributeValue("X");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.N.Should().Be("FF");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void Long_ToAttributeValue_UsesInvariantCultureAndFormatString()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 123456789L.ToAttributeValue("N0");

        attribute.N.Should().Be("123,456,789");
    }

    [Fact]
    public void NullableLong_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        long? value = 4095;
        long? nullValue = null;

        var attribute = value.ToAttributeValue("X");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.N.Should().Be("FFF");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void Float_ToAttributeValue_UsesInvariantCulture()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 12.5f.ToAttributeValue();

        attribute.N.Should().Be("12.5");
    }

    [Fact]
    public void NullableFloat_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        float? value = 12.5f;
        float? nullValue = null;

        var attribute = value.ToAttributeValue("0.00");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.N.Should().Be("12.50");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void Double_ToAttributeValue_UsesInvariantCulture()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 1234.5d.ToAttributeValue();

        attribute.N.Should().Be("1234.5");
    }

    [Fact]
    public void NullableDouble_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        double? value = 1234.5d;
        double? nullValue = null;

        var attribute = value.ToAttributeValue("0.000");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.N.Should().Be("1234.500");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void Decimal_ToAttributeValue_UsesInvariantCulture()
    {
        using var _ = new CultureScope("de-DE");

        var attribute = 1234.5m.ToAttributeValue();

        attribute.N.Should().Be("1234.5");
    }

    [Fact]
    public void NullableDecimal_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        decimal? value = 1234.5m;
        decimal? nullValue = null;

        var attribute = value.ToAttributeValue("0.000");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.N.Should().Be("1234.500");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void Guid_ToAttributeValue_UsesDefaultDFormat()
    {
        var value = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");

        var attribute = value.ToAttributeValue();

        attribute.S.Should().Be("00112233-4455-6677-8899-aabbccddeeff");
    }

    [Fact]
    public void NullableGuid_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        Guid? value = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
        Guid? nullValue = null;

        var attribute = value.ToAttributeValue("N");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.S.Should().Be("00112233445566778899aabbccddeeff");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void DateTime_ToAttributeValue_UsesDefaultRoundTripFormat()
    {
        var value = new DateTime(2025, 4, 7, 12, 34, 56, DateTimeKind.Utc);

        var attribute = value.ToAttributeValue();

        attribute.S.Should().Be(value.ToString("o", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullableDateTime_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        DateTime? value = new DateTime(2025, 4, 7, 12, 34, 56, DateTimeKind.Utc);
        DateTime? nullValue = null;

        var attribute = value.ToAttributeValue("yyyyMMdd");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.S.Should().Be("20250407");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void DateTimeOffset_ToAttributeValue_UsesDefaultRoundTripFormat()
    {
        var value = new DateTimeOffset(2025, 4, 7, 12, 34, 56, TimeSpan.FromHours(2));

        var attribute = value.ToAttributeValue();

        attribute.S.Should().Be(value.ToString("o", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullableDateTimeOffset_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        DateTimeOffset? value = new DateTimeOffset(2025, 4, 7, 12, 34, 56, TimeSpan.Zero);
        DateTimeOffset? nullValue = null;

        var attribute = value.ToAttributeValue("yyyy-MM-dd zzz");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.S.Should().Be("2025-04-07 +00:00");
        AssertNullAttribute(nullAttribute);
    }

    [Fact]
    public void TimeSpan_ToAttributeValue_UsesDefaultConstantFormat()
    {
        var value = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3);

        var attribute = value.ToAttributeValue();

        attribute.S.Should().Be(value.ToString("c", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullableTimeSpan_ToAttributeValue_UsesFormatStringOrNullAttribute()
    {
        TimeSpan? value =
            TimeSpan.FromHours(26) + TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(4);
        TimeSpan? nullValue = null;

        var attribute = value.ToAttributeValue("g");
        var nullAttribute = nullValue.ToAttributeValue();

        attribute.S.Should().Be(value.Value.ToString("g", CultureInfo.InvariantCulture));
        AssertNullAttribute(nullAttribute);
    }

    private static void AssertNullAttribute(AttributeValue attribute)
    {
        attribute.NULL.Should().BeTrue();
        attribute.S.Should().BeNull();
        attribute.N.Should().BeNull();
        attribute.BOOL.Should().BeNull();
    }

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
