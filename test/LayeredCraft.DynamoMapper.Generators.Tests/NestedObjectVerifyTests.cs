namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class NestedObjectVerifyTests
{
    [Fact]
    public async Task NestedObject_SimpleInline() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address ShippingAddress { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                    public string PostalCode { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_NullableInline() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address? BillingAddress { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_MapperBased() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class AddressMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Address source);

                    public static partial Address FromItem(Dictionary<string, AttributeValue> item);
                }

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address ShippingAddress { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                    public string PostalCode { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_MapperBased_MissingFromMethod_FallsBackToInline() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class AddressMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Address source);
                }

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address ShippingAddress { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                    public string PostalCode { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_MultiLevel() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Customer Customer { get; set; }
                }

                public class Customer
                {
                    public string Name { get; set; }
                    public Address Address { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_WithScalarTypes() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ProductMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Product source);

                    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Product
                {
                    public string Id { get; set; }
                    public string Name { get; set; }
                    public ProductDetails Details { get; set; }
                }

                public class ProductDetails
                {
                    public decimal Price { get; set; }
                    public int StockCount { get; set; }
                    public DateTime LastUpdated { get; set; }
                    public bool IsActive { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_WithDotNotationOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField("ShippingAddress.Line1", AttributeName = "addr_line1")]
                [DynamoField("ShippingAddress.City", AttributeName = "addr_city")]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address ShippingAddress { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                    public string PostalCode { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    // ==================== NESTED COLLECTION TESTS ====================

    [Fact]
    public async Task NestedCollection_ListOfNestedObjects_Inline() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public List<LineItem> Items { get; set; }
                }

                public class LineItem
                {
                    public string ProductId { get; set; }
                    public int Quantity { get; set; }
                    public decimal Price { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_ListOfNestedObjects_MapperBased() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class LineItemMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(LineItem source);

                    public static partial LineItem FromItem(Dictionary<string, AttributeValue> item);
                }

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public List<LineItem> Items { get; set; }
                }

                public class LineItem
                {
                    public string ProductId { get; set; }
                    public int Quantity { get; set; }
                    public decimal Price { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_NullableListOfNestedObjects() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public List<LineItem>? Items { get; set; }
                }

                public class LineItem
                {
                    public string ProductId { get; set; }
                    public int Quantity { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_ArrayOfNestedObjects() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public LineItem[] Items { get; set; }
                }

                public class LineItem
                {
                    public string ProductId { get; set; }
                    public int Quantity { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_DictionaryOfNestedObjects_Inline() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class CatalogMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Catalog source);

                    public static partial Catalog FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Catalog
                {
                    public string Id { get; set; }
                    public Dictionary<string, Product> Products { get; set; }
                }

                public class Product
                {
                    public string Name { get; set; }
                    public decimal Price { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_DictionaryOfNestedObjects_MapperBased() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ProductMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Product source);

                    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                [DynamoMapper]
                public static partial class CatalogMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Catalog source);

                    public static partial Catalog FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Catalog
                {
                    public string Id { get; set; }
                    public Dictionary<string, Product> Products { get; set; }
                }

                public class Product
                {
                    public string Name { get; set; }
                    public decimal Price { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_ListWithNestedObjectContainingScalarTypes() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class EventLogMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(EventLog source);

                    public static partial EventLog FromItem(Dictionary<string, AttributeValue> item);
                }

                public class EventLog
                {
                    public string Id { get; set; }
                    public List<LogEntry> Entries { get; set; }
                }

                public class LogEntry
                {
                    public DateTime Timestamp { get; set; }
                    public string Message { get; set; }
                    public int Severity { get; set; }
                    public bool IsError { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    // ==================== DIAGNOSTIC TESTS ====================

    [Fact]
    public async Task NestedObject_CycleDetected_ShouldFail_DM0006() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class PersonMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Person source);

                    public static partial Person FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Person
                {
                    public string Name { get; set; }
                    public Person Parent { get; set; }  // Self-referencing cycle
                }
                """,
                ExpectedDiagnosticId = "DM0006",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_IndirectCycleDetected_ShouldFail_DM0006() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class NodeAMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(NodeA source);

                    public static partial NodeA FromItem(Dictionary<string, AttributeValue> item);
                }

                public class NodeA
                {
                    public string Id { get; set; }
                    public NodeB Child { get; set; }  // A -> B -> A cycle
                }

                public class NodeB
                {
                    public string Id { get; set; }
                    public NodeA Parent { get; set; }  // Back reference creates cycle
                }
                """,
                ExpectedDiagnosticId = "DM0006",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedCollection_CycleDetected_ShouldFail_DM0006() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class NodeMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Node source);

                    public static partial Node FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Node
                {
                    public string Name { get; set; }
                    public List<Node> Children { get; set; }
                }
                """,
                ExpectedDiagnosticId = "DM0006",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_InvalidDotNotationPath_ShouldFail_DM0008() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField("Address.NonExistentProperty", AttributeName = "bad_path")]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address Address { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                }
                """,
                ExpectedDiagnosticId = "DM0008",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NestedObject_InvalidDotNotationPath_NonExistentProperty_ShouldFail_DM0008() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField("NonExistentProperty.Line1", AttributeName = "bad_path")]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string Id { get; set; }
                    public Address Address { get; set; }
                }

                public class Address
                {
                    public string Line1 { get; set; }
                    public string City { get; set; }
                }
                """,
                ExpectedDiagnosticId = "DM0008",
            },
            TestContext.Current.CancellationToken
        );
}
