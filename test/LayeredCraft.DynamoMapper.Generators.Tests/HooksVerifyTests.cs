namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class HooksVerifyTests
{
    [Fact]
    public async Task Hooks_NoHooks_PreservesExpressionBody() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ProductMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
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
    public async Task Hooks_AfterToItem_Only() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ProductMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

                    static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item);
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
    public async Task Hooks_BeforeToItem_Only() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ProductMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

                    static partial void BeforeToItem(Product source, Dictionary<string, AttributeValue> item);
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
    public async Task Hooks_AfterFromItem_Only() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class UserMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(User source);
                    public static partial User FromItem(Dictionary<string, AttributeValue> item);

                    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref User entity);
                }

                public class User
                {
                    public string FirstName { get; set; }
                    public string LastName { get; set; }
                    public string Email { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task Hooks_BeforeFromItem_Only() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

                    static partial void BeforeFromItem(Dictionary<string, AttributeValue> item);
                }

                public class Order
                {
                    public string CustomerId { get; set; }
                    public decimal Total { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task Hooks_AllFour() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class OrderMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

                    static partial void BeforeToItem(Order source, Dictionary<string, AttributeValue> item);
                    static partial void AfterToItem(Order source, Dictionary<string, AttributeValue> item);
                    static partial void BeforeFromItem(Dictionary<string, AttributeValue> item);
                    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref Order entity);
                }

                public class Order
                {
                    public string CustomerId { get; set; }
                    public string OrderId { get; set; }
                    public decimal Total { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task Hooks_ToItemOnly_Mapper_WithAfterToItem() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ProductMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Product source);

                    static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item);
                }

                public class Product
                {
                    public string Name { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );
}
