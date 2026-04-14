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
    public async Task Hooks_ToItemOnly_Mapper_WithAfterToItem() =>
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

    [Fact]
    public async Task Hooks_FromOnly_BothFromHooks() => await GeneratorTestHelpers.Verify(
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
                    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

                    static partial void BeforeFromItem(Dictionary<string, AttributeValue> item);
                    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref Product entity);
                }

                public class Product
                {
                    public string Name { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task Hooks_ZeroPropertyToItem_WithBothToHooks() =>
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
                    public static partial class EmptyModelMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(EmptyModel source);

                        static partial void BeforeToItem(EmptyModel source, Dictionary<string, AttributeValue> item);
                        static partial void AfterToItem(EmptyModel source, Dictionary<string, AttributeValue> item);
                    }

                    public class EmptyModel
                    {
                    }
                    """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Hooks_ImplementationOnlyExtendedPartialHook_IsDetected() =>
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
                    public static partial class ProductMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Product source);

                        private static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item);
                    }

                    public static partial class ProductMapper
                    {
                        private static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item) { }
                    }

                    public class Product
                    {
                        public string Name { get; set; }
                    }
                    """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Hooks_NonPartial_AfterToItem_IsRecognized() =>
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
                    public static partial class ProductMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Product source);
                        public static partial Product FromItem(Dictionary<string, AttributeValue> item);

                        static void AfterToItem(Product source, Dictionary<string, AttributeValue> item) { }
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
    public async Task Hooks_NonPartial_AllFour_IsRecognized() => await GeneratorTestHelpers.Verify(
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

                    static void BeforeToItem(Order source, Dictionary<string, AttributeValue> item) { }
                    static void AfterToItem(Order source, Dictionary<string, AttributeValue> item) { }
                    static void BeforeFromItem(Dictionary<string, AttributeValue> item) { }
                    static void AfterFromItem(Dictionary<string, AttributeValue> item, ref Order entity) { }
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
    public async Task Hooks_NonStaticHook_ShouldWarn_DM0402() =>
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
                    public partial class ProductMapper
                    {
                        public partial Dictionary<string, AttributeValue> ToItem(Product source);

                        partial void BeforeToItem(Product source, Dictionary<string, AttributeValue> item);
                    }

                    public class Product
                    {
                        public string Name { get; set; }
                    }
                    """,
                ExpectedDiagnosticId = "DM0402",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Hooks_WrongParameterTypes_ShouldWarn_DM0403() =>
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
                    public static partial class ProductMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Product source);

                        static partial void BeforeToItem(Order source, Dictionary<string, AttributeValue> item);
                    }

                    public class Product
                    {
                        public string Name { get; set; }
                    }

                    public class Order
                    {
                        public string Id { get; set; }
                    }
                    """,
                ExpectedDiagnosticId = "DM0403",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Hooks_InvalidSignature_ShouldWarn_DM0401() =>
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
                    public static partial class ProductMapper
                    {
                        public static partial Dictionary<string, AttributeValue> ToItem(Product source);

                        static partial void BeforeToItem(Product source);
                    }

                    public class Product
                    {
                        public string Name { get; set; }
                    }
                    """,
                ExpectedDiagnosticId = "DM0401",
            },
            TestContext.Current.CancellationToken
        );
}
