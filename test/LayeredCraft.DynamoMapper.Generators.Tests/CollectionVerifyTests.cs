namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class CollectionVerifyTests
{
    [Fact]
    public async Task Collection_ListOfString() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public List<string> Tags { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_ArrayOfString() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public string[] Tags { get; set; } = Array.Empty<string>();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_ListOfInt() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public List<int> Scores { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    // ==================== DICTIONARY TESTS ====================

    [Fact]
    public async Task Collection_DictionaryStringToInt() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public Dictionary<string, int> Metadata { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_DictionaryStringToString() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public Dictionary<string, string> Attributes { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    // ==================== SET TESTS ====================

    [Fact]
    public async Task Collection_HashSetOfString() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public HashSet<string> Categories { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_HashSetOfInt() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public HashSet<int> Numbers { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_HashSetOfByteArray() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public HashSet<byte[]> Payloads { get; set; } = new();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    // ==================== INTERFACE TESTS ====================

    [Fact]
    public async Task Collection_IEnumerableOfString() =>
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
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public IEnumerable<string> Items { get; set; } = new List<string>();
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    // ==================== NEGATIVE TESTS ====================

    [Fact]
    public async Task Collection_NestedList_ShouldFail() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public List<List<string>> NestedList { get; set; } = new();
                }
                """,
                ExpectedDiagnosticId = "DM0003",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_ComplexElementType_ShouldFail() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public List<CustomClass> Items { get; set; } = new();
                }

                public class CustomClass
                {
                    public string Name { get; set; }
                }
                """,
                ExpectedDiagnosticId = "DM0003",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_DictionaryWithIntKey_ShouldFail() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public Dictionary<int, string> InvalidDict { get; set; } = new();
                }
                """,
                ExpectedDiagnosticId = "DM0004",
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Collection_IncompatibleKindOverride_ShouldFail() =>
        await GeneratorTestHelpers.VerifyFailure(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(Entity.Numbers), Kind = DynamoKind.S)]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public List<int> Numbers { get; set; } = new();
                }
                """,
                ExpectedDiagnosticId = "DM0005",
            },
            TestContext.Current.CancellationToken
        );
}
