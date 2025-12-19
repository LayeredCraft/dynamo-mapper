namespace LayeredCraft.DynamoMapper.TestKit.Attributes;

public sealed class DynamoMapperAutoDataAttribute() : AutoDataAttribute(CreateFixture)
{
    internal static IFixture CreateFixture()
    {
        return BaseFixtureFactory.CreateFixture(fixture =>
        {
            // Add common customizations for DynamoMapper tests
            // e.g., fixture.Customize(new SomeDynamoMapperSpecificCustomization());
        });
    }
}
public sealed class InlineDynamoMapperAutoDataAttribute(params object[] values)
    : InlineAutoDataAttribute(DynamoMapperAutoDataAttribute.CreateFixture, values)
{
    // This class allows for inline data to be combined with the DynamoMapperAutoDataAttribute customizations
}