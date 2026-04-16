namespace LayeredCraft.DynamoMapper.IntegrationTests;

public class DeepNestedMapperTests
{
    private static DeepNestedAccount CreateModel() => new()
    {
        AccountId = "acct-001",
        Profile =
            new DeepNestedUserProfile
            {
                DisplayName = "Alice",
                Roles = ["admin", "viewer"],
                Orders =
                [
                    new DeepNestedPurchaseOrder
                    {
                        OrderId = "ord-001",
                        Total = 150.75m,
                        Destination =
                            new DeepNestedShippingAddress
                            {
                                Street = "123 Main St",
                                City = "Springfield",
                                PostalCode = "12345",
                            },
                        Lines =
                        [
                            new DeepNestedLineItem
                            {
                                Sku = "SKU-A", Qty = 2, Price = 49.99m,
                            },
                            new DeepNestedLineItem
                            {
                                Sku = "SKU-B", Qty = 1, Price = 50.77m,
                            },
                        ],
                    },
                    new DeepNestedPurchaseOrder
                    {
                        OrderId = "ord-002",
                        Total = 9.99m,
                        Destination =
                            new DeepNestedShippingAddress
                            {
                                Street = "456 Elm Ave",
                                City = "Shelbyville",
                                PostalCode = "67890",
                            },
                        Lines =
                        [
                            new DeepNestedLineItem
                            {
                                Sku = "SKU-C", Qty = 1, Price = 9.99m,
                            },
                        ],
                    },
                ],
            },
    };

    [Fact]
    public void DeepNested_RoundTrip_PreservesAllValues()
    {
        var expected = CreateModel();

        var item = DeepNestedAccountMapper.ToItem(expected);
        var actual = DeepNestedAccountMapper.FromItem(item);

        actual.AccountId.Should().Be(expected.AccountId);

        actual.Profile.DisplayName.Should().Be(expected.Profile.DisplayName);
        actual.Profile.Roles.Should().Equal(expected.Profile.Roles);

        actual.Profile.Orders.Should().HaveCount(expected.Profile.Orders.Count);

        for (var i = 0; i < expected.Profile.Orders.Count; i++)
        {
            var expectedOrder = expected.Profile.Orders[i];
            var actualOrder = actual.Profile.Orders[i];

            actualOrder.OrderId.Should().Be(expectedOrder.OrderId);
            actualOrder.Total.Should().Be(expectedOrder.Total);

            actualOrder.Destination.Street.Should().Be(expectedOrder.Destination.Street);
            actualOrder.Destination.City.Should().Be(expectedOrder.Destination.City);
            actualOrder.Destination.PostalCode.Should().Be(expectedOrder.Destination.PostalCode);

            actualOrder.Lines.Should().HaveCount(expectedOrder.Lines.Count);
            for (var j = 0; j < expectedOrder.Lines.Count; j++)
            {
                actualOrder.Lines[j].Sku.Should().Be(expectedOrder.Lines[j].Sku);
                actualOrder.Lines[j].Qty.Should().Be(expectedOrder.Lines[j].Qty);
                actualOrder.Lines[j].Price.Should().Be(expectedOrder.Lines[j].Price);
            }
        }
    }

    [Fact]
    public void DeepNested_ToItem_ProducesCorrectAttributeShape()
    {
        var model = CreateModel();

        var item = DeepNestedAccountMapper.ToItem(model);

        // Root scalar
        item["accountId"].S.Should().Be("acct-001");

        // Level 1 owned type
        item["profile"].M.Should().ContainKey("displayName");
        item["profile"].M["displayName"].S.Should().Be("Alice");

        // Level 1 scalar list inside owned type
        item["profile"].M["roles"].L.Should().HaveCount(2);
        item["profile"].M["roles"].L[0].S.Should().Be("admin");

        // Level 1 → level 2: list of complex objects
        item["profile"].M["orders"].L.Should().HaveCount(2);

        var firstOrder = item["profile"].M["orders"].L[0].M;
        firstOrder["orderId"].S.Should().Be("ord-001");
        firstOrder["total"].N.Should().Be("150.75");

        // Level 2 → level 3: owned complex type inside a list element
        firstOrder["destination"].M["street"].S.Should().Be("123 Main St");
        firstOrder["destination"].M["city"].S.Should().Be("Springfield");

        // Level 2 → level 3: list of complex objects inside a list element
        firstOrder["lines"].L.Should().HaveCount(2);
        firstOrder["lines"].L[0].M["sku"].S.Should().Be("SKU-A");
        firstOrder["lines"].L[0].M["qty"].N.Should().Be("2");
        firstOrder["lines"].L[0].M["price"].N.Should().Be("49.99");
    }

    [Fact]
    public void DeepNested_FromItem_WithEmptyCollections_ReturnsEmptyLists()
    {
        var model =
            new DeepNestedAccount
            {
                AccountId = "acct-empty",
                Profile =
                    new DeepNestedUserProfile { DisplayName = "Bob", Roles = [], Orders = [] },
            };

        var item = DeepNestedAccountMapper.ToItem(model);
        var actual = DeepNestedAccountMapper.FromItem(item);

        actual.Profile.Roles.Should().BeEmpty();
        actual.Profile.Orders.Should().BeEmpty();
    }
}
