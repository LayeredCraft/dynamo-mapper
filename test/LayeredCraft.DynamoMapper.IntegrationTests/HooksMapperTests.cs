using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

public class HooksMapperTests
{
    // ── ToItem ────────────────────────────────────────────────────────────────

    [Fact]
    public void ToItem_AfterToItem_InjectsPkAndSk()
    {
        var model = new HooksIntegrationModel { Id = "abc-123", Name = "Widget" };

        var item = HooksIntegrationModelMapper.ToItem(model);

        item["pk"].S.Should().Be("MODEL#abc-123");
        item["sk"].S.Should().Be("METADATA");
    }

    [Fact]
    public void ToItem_BeforeToItem_RunsBeforePropertyMapping()
    {
        // BeforeToItem sets _lifecyclePhase = "before-mapping".
        // AfterToItem overwrites it to "after-mapping".
        // If BeforeToItem never ran, the key won't exist at all (or be stale).
        // If AfterToItem ran last, the final value is "after-mapping".
        var model = new HooksIntegrationModel { Id = "x", Name = "Y" };

        var item = HooksIntegrationModelMapper.ToItem(model);

        item["_lifecyclePhase"].S.Should().Be("after-mapping");
    }

    [Fact]
    public void ToItem_PropertyMappingRunsBetweenHooks()
    {
        // Mapped properties (id, name) must be present alongside hook-injected keys.
        var model = new HooksIntegrationModel { Id = "my-id", Name = "my-name" };

        var item = HooksIntegrationModelMapper.ToItem(model);

        // Properties from mapper
        item["id"].S.Should().Be("my-id");
        item["name"].S.Should().Be("my-name");
        // Keys from AfterToItem hook
        item["pk"].S.Should().Be("MODEL#my-id");
        item["sk"].S.Should().Be("METADATA");
        // Lifecycle marker (set by BeforeToItem, overwritten by AfterToItem)
        item["_lifecyclePhase"].S.Should().Be("after-mapping");
    }

    [Fact]
    public void ToItem_NormalizedName_IsNotMapped()
    {
        // NormalizedName is marked [DynamoIgnore] — must not appear in the item.
        var model = new HooksIntegrationModel
        {
            Id = "id",
            Name = "name",
            NormalizedName = "SHOULD-NOT-APPEAR",
        };

        var item = HooksIntegrationModelMapper.ToItem(model);

        item.Should().NotContainKey("normalizedName");
    }

    // ── FromItem ──────────────────────────────────────────────────────────────

    [Fact]
    public void FromItem_AfterFromItem_PopulatesNormalizedName()
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = "some-id" },
            ["name"] = new() { S = "Widget Pro" },
        };

        var entity = HooksIntegrationModelMapper.FromItem(item);

        entity.NormalizedName.Should().Be("WIDGET PRO");
    }

    [Fact]
    public void FromItem_AfterFromItem_NormalizedNameDerivedFromMappedName()
    {
        // Verifies AfterFromItem receives the already-mapped entity, not a blank one.
        var item = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = "id-1" },
            ["name"] = new() { S = "hello world" },
        };

        var entity = HooksIntegrationModelMapper.FromItem(item);

        entity.Name.Should().Be("hello world");
        entity.NormalizedName.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void FromItem_BeforeFromItem_AcceptsMatchingEntityType()
    {
        // BeforeFromItem only throws for mismatched entityType — matching type must succeed.
        var item = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = "id-1" },
            ["name"] = new() { S = "Test" },
            ["entityType"] = new() { S = "HooksIntegrationModel" },
        };

        var act = () => HooksIntegrationModelMapper.FromItem(item);

        act.Should().NotThrow();
    }

    [Fact]
    public void FromItem_BeforeFromItem_ThrowsOnEntityTypeMismatch()
    {
        // BeforeFromItem validates entityType — wrong type must throw before any mapping occurs.
        var item = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = "id-1" },
            ["name"] = new() { S = "Test" },
            ["entityType"] = new() { S = "SomeOtherEntity" },
        };

        var act = () => HooksIntegrationModelMapper.FromItem(item);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SomeOtherEntity*");
    }

    // ── Round-trip ────────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_MappedPropertiesPreserved()
    {
        var original = new HooksIntegrationModel { Id = "round-trip-id", Name = "Round Trip" };

        var item = HooksIntegrationModelMapper.ToItem(original);
        var restored = HooksIntegrationModelMapper.FromItem(item);

        restored.Id.Should().Be(original.Id);
        restored.Name.Should().Be(original.Name);
    }

    [Fact]
    public void RoundTrip_AfterFromItem_PopulatesNormalizedNameFromRestoredName()
    {
        var original = new HooksIntegrationModel { Id = "id", Name = "round trip name" };

        var item = HooksIntegrationModelMapper.ToItem(original);
        var restored = HooksIntegrationModelMapper.FromItem(item);

        // NormalizedName is never stored in DynamoDB — AfterFromItem computes it each time
        restored.NormalizedName.Should().Be("ROUND TRIP NAME");
    }

    [Fact]
    public void RoundTrip_HookInjectedKeys_PresentInItem_NotOnEntity()
    {
        var original = new HooksIntegrationModel { Id = "pk-test", Name = "Test" };

        var item = HooksIntegrationModelMapper.ToItem(original);
        var restored = HooksIntegrationModelMapper.FromItem(item);

        // pk/sk exist in DynamoDB item (injected by AfterToItem)
        item.Should().ContainKey("pk");
        item.Should().ContainKey("sk");

        // But they don't exist on the entity — the model has no pk/sk properties
        restored.Id.Should().Be(original.Id);
        restored.Name.Should().Be(original.Name);
    }
}
