using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

public class NonPartialHooksMapperTests
{
    // ── ToItem ────────────────────────────────────────────────────────────────

    [Fact]
    public void ToItem_AfterToItem_InjectsPkAndSk()
    {
        var model = new NonPartialHooksModel { Id = "abc-123", Name = "Widget" };

        var item = NonPartialHooksModelMapper.ToItem(model);

        item["pk"].S.Should().Be("MODEL#abc-123");
        item["sk"].S.Should().Be("METADATA");
    }

    [Fact]
    public void ToItem_BeforeToItem_RunsBeforePropertyMapping()
    {
        var model = new NonPartialHooksModel { Id = "x", Name = "Y" };

        var item = NonPartialHooksModelMapper.ToItem(model);

        // AfterToItem overwrites the marker set by BeforeToItem — proves both ran in order
        item["_lifecyclePhase"].S.Should().Be("after-mapping");
    }

    [Fact]
    public void ToItem_PropertyMappingRunsBetweenHooks()
    {
        var model = new NonPartialHooksModel { Id = "my-id", Name = "my-name" };

        var item = NonPartialHooksModelMapper.ToItem(model);

        item["id"].S.Should().Be("my-id");
        item["name"].S.Should().Be("my-name");
        item["pk"].S.Should().Be("MODEL#my-id");
        item["sk"].S.Should().Be("METADATA");
        item["_lifecyclePhase"].S.Should().Be("after-mapping");
    }

    // ── FromItem ──────────────────────────────────────────────────────────────

    [Fact]
    public void FromItem_AfterFromItem_PopulatesNormalizedName()
    {
        var item =
            new Dictionary<string, AttributeValue>
            {
                ["id"] = new() { S = "some-id" }, ["name"] = new() { S = "Widget Pro" },
            };

        var entity = NonPartialHooksModelMapper.FromItem(item);

        entity.NormalizedName.Should().Be("WIDGET PRO");
    }

    [Fact]
    public void FromItem_BeforeFromItem_ThrowsOnEntityTypeMismatch()
    {
        var item =
            new Dictionary<string, AttributeValue>
            {
                ["id"] = new() { S = "id-1" },
                ["name"] = new() { S = "Test" },
                ["entityType"] = new() { S = "SomeOtherEntity" },
            };

        var act = () => NonPartialHooksModelMapper.FromItem(item);

        act.Should().Throw<InvalidOperationException>().WithMessage("*SomeOtherEntity*");
    }

    // ── Round-trip ────────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_MappedPropertiesPreserved()
    {
        var original = new NonPartialHooksModel { Id = "round-trip-id", Name = "Round Trip" };

        var item = NonPartialHooksModelMapper.ToItem(original);
        var restored = NonPartialHooksModelMapper.FromItem(item);

        restored.Id.Should().Be(original.Id);
        restored.Name.Should().Be(original.Name);
        restored.NormalizedName.Should().Be("ROUND TRIP");
    }
}
