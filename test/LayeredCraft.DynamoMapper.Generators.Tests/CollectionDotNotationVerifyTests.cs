namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class CollectionDotNotationVerifyTests
{
    [Fact]
    public async Task Collection_ListOfNestedObjects_WithDotNotationFormatOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode =
                    """
                    using System;
                    using System.Collections.Generic;
                    using Amazon.DynamoDBv2.Model;
                    using LayeredCraft.DynamoMapper.Runtime;

                    namespace MyNamespace;

                    [DynamoMapper]
                    [DynamoField("Contacts.VerifiedAt", Format = "yyyy-MM-dd")]
                    public static partial class CustomerMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Customer source);
                        public static partial Customer FromItem(Dictionary<string, AttributeValue> item);
                    }

                    public class Customer
                    {
                        public string Id { get; set; }
                        public List<CustomerContact> Contacts { get; set; }
                    }

                    public class CustomerContact
                    {
                        public string Name { get; set; }
                        public DateTime VerifiedAt { get; set; }
                    }
                    """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_ListOfNestedObjects_WithDotNotationAttributeNameOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode =
                    """
                    using System.Collections.Generic;
                    using Amazon.DynamoDBv2.Model;
                    using LayeredCraft.DynamoMapper.Runtime;

                    namespace MyNamespace;

                    [DynamoMapper]
                    [DynamoField("Items.ProductId", AttributeName = "product_id")]
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
                    }
                    """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_ArrayOfNestedObjects_WithDotNotationOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode =
                    """
                    using System.Collections.Generic;
                    using Amazon.DynamoDBv2.Model;
                    using LayeredCraft.DynamoMapper.Runtime;

                    namespace MyNamespace;

                    [DynamoMapper]
                    [DynamoField("Items.ProductId", AttributeName = "product_id")]
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
    public async Task Collection_DictionaryOfNestedObjects_WithDotNotationOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode =
                    """
                    using System;
                    using System.Collections.Generic;
                    using Amazon.DynamoDBv2.Model;
                    using LayeredCraft.DynamoMapper.Runtime;

                    namespace MyNamespace;

                    [DynamoMapper]
                    [DynamoField("ProductMap.CreatedAt", Format = "yyyy-MM-dd")]
                    public static partial class CatalogMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Catalog source);
                        public static partial Catalog FromItem(Dictionary<string, AttributeValue> item);
                    }

                    public class Catalog
                    {
                        public string Id { get; set; }
                        public Dictionary<string, OrderItem> ProductMap { get; set; }
                    }

                    public class OrderItem
                    {
                        public string Name { get; set; }
                        public DateTime CreatedAt { get; set; }
                    }
                    """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_ListOfNestedObjects_WithMultipleDotNotationOverrides() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode =
                    """
                    using System;
                    using System.Collections.Generic;
                    using Amazon.DynamoDBv2.Model;
                    using LayeredCraft.DynamoMapper.Runtime;

                    namespace MyNamespace;

                    [DynamoMapper]
                    [DynamoField("Contacts.Name", AttributeName = "contact_name")]
                    [DynamoField("Contacts.VerifiedAt", Format = "yyyy-MM-dd")]
                    public static partial class CustomerMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Customer source);
                        public static partial Customer FromItem(Dictionary<string, AttributeValue> item);
                    }

                    public class Customer
                    {
                        public string Id { get; set; }
                        public List<CustomerContact> Contacts { get; set; }
                    }

                    public class CustomerContact
                    {
                        public string Name { get; set; }
                        public DateTime VerifiedAt { get; set; }
                    }
                    """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task
        Collection_ListOfNestedObjects_WithDotPath_InvalidCollectionElementProperty_ShouldFail_DM0008() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode =
                    """
                    using System.Collections.Generic;
                    using Amazon.DynamoDBv2.Model;
                    using LayeredCraft.DynamoMapper.Runtime;

                    namespace MyNamespace;

                    [DynamoMapper]
                    [DynamoField("Contacts.NonExistentProp", AttributeName = "bad_path")]
                    public static partial class CustomerMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Customer source);
                        public static partial Customer FromItem(Dictionary<string, AttributeValue> item);
                    }

                    public class Customer
                    {
                        public string Id { get; set; }
                        public List<CustomerContact> Contacts { get; set; }
                    }

                    public class CustomerContact
                    {
                        public string Name { get; set; }
                    }
                    """,
                ExpectedDiagnosticId = "DM0008",
            },
            TestContext.Current.CancellationToken
        );
}
