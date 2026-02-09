namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class InheritanceVerifyTests
{
    [Fact]
    public async Task Inheritance_Default_DoesNotIncludeBaseProperties() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class BaseEntity
                {
                    internal string Id { get; set; } = string.Empty;
                }

                internal class Order : BaseEntity
                {
                    internal string Name { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_OptIn_IncludesBaseProperties() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(IncludeBaseClassProperties = true)]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class BaseEntity
                {
                    internal string Id { get; set; } = string.Empty;
                }

                internal class Order : BaseEntity
                {
                    internal string Name { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_OptIn_CanIgnoreBaseProperty() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(IncludeBaseClassProperties = true)]
                [DynamoIgnore(nameof(BaseEntity.Id))]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class BaseEntity
                {
                    internal string Id { get; set; } = string.Empty;
                }

                internal class Order : BaseEntity
                {
                    internal string Name { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_OptIn_CanOverrideFieldOnBaseProperty() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(IncludeBaseClassProperties = true)]
                [DynamoField(nameof(BaseEntity.Id), AttributeName = "pk")]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class BaseEntity
                {
                    internal string Id { get; set; } = string.Empty;
                }

                internal class Order : BaseEntity
                {
                    internal string Name { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_DotNotation_BaseProperty_WithoutOptIn_Fails() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                ExpectedDiagnosticId = "DM0008",
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField("Address.Line1", AttributeName = "line_1")]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Order
                {
                    internal Address Address { get; set; } = new();
                }

                internal class AddressBase
                {
                    internal string Line1 { get; set; } = string.Empty;
                }

                internal class Address : AddressBase
                {
                    internal string City { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_DotNotation_BaseProperty_WithOptIn_Succeeds() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(IncludeBaseClassProperties = true)]
                [DynamoField("Address.Line1", AttributeName = "line_1")]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Order
                {
                    internal Address Address { get; set; } = new();
                }

                internal class AddressBase
                {
                    internal string Line1 { get; set; } = string.Empty;
                }

                internal class Address : AddressBase
                {
                    internal string City { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_OptIn_Shadowing_DerivedWins() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(IncludeBaseClassProperties = true)]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class BaseEntity
                {
                    internal string Id { get; set; } = string.Empty;
                }

                internal class Order : BaseEntity
                {
                    internal new string Id { get; set; } = string.Empty;
                    internal string Name { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Inheritance_OptIn_ConstructorParameter_CanBindBaseProperty() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(IncludeBaseClassProperties = true)]
                internal static partial class OrderMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);
                    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class BaseEntity
                {
                    internal BaseEntity(string id) => Id = id;
                    internal string Id { get; }
                }

                internal class Order : BaseEntity
                {
                    internal Order(string id, string name) : base(id) => Name = name;
                    internal string Name { get; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
