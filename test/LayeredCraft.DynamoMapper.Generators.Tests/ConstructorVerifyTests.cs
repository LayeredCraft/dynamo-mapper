namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class ConstructorVerifyTests
{
    [Fact]
    public async Task Constructor_RecordWithPrimaryConstructor() =>
        await GeneratorTestHelpers.Verify(
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

                public record Person(string FirstName, string LastName, int Age);
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_ClassWithReadOnlyPropertiesAndConstructor() =>
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

                public class Product
                {
                    public Product(string name, decimal price, int quantity)
                    {
                        Name = name;
                        Price = price;
                        Quantity = quantity;
                    }

                    public string Name { get; }
                    public decimal Price { get; }
                    public int Quantity { get; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_ClassWithAttributedConstructor() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class UserMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(User source);
                    public static partial User FromItem(Dictionary<string, AttributeValue> item);
                }

                public class User
                {
                    public User()
                    {
                        Id = string.Empty;
                        Name = string.Empty;
                    }

                    [DynamoMapperConstructor]
                    public User(string id, string name)
                    {
                        Id = id;
                        Name = name;
                    }

                    public string Id { get; set; }
                    public string Name { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_ClassWithSettablePropertiesPreferredOverConstructor() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class CustomerMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Customer source);
                    public static partial Customer FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Customer
                {
                    public Customer()
                    {
                    }

                    public Customer(string name, int age)
                    {
                        Name = name;
                        Age = age;
                    }

                    public string Name { get; set; }
                    public int Age { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_ClassWithMultipleConstructorsPicksLargest() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class EntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public Entity(string id)
                    {
                        Id = id;
                        Name = string.Empty;
                        Count = 0;
                    }

                    public Entity(string id, string name)
                    {
                        Id = id;
                        Name = name;
                        Count = 0;
                    }

                    public Entity(string id, string name, int count)
                    {
                        Id = id;
                        Name = name;
                        Count = count;
                    }

                    public string Id { get; }
                    public string Name { get; }
                    public int Count { get; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_ClassWithInitOnlyProperties() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ConfigMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Config source);
                    public static partial Config FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Config
                {
                    public string Name { get; init; }
                    public int Value { get; init; }
                    public bool Enabled { get; init; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_RecordStructWithPrimaryConstructor() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class PointMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Point source);
                    public static partial Point FromItem(Dictionary<string, AttributeValue> item);
                }

                public record struct Point(int X, int Y, int Z);
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_MixedReadOnlyAndSettableProperties() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class EntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(Entity source);
                    public static partial Entity FromItem(Dictionary<string, AttributeValue> item);
                }

                public class Entity
                {
                    public Entity(string id)
                    {
                        Id = id;
                    }

                    public string Id { get; }
                    public string Name { get; set; }
                    public int Count { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_NoParameterlessConstructorWithInitProperties() =>
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
                    public Order(string id)
                    {
                        Id = id;
                    }

                    public string Id { get; }
                    public string CustomerName { get; init; }
                    public decimal Total { get; init; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_ComplexNameMatching() =>
        await GeneratorTestHelpers.Verify(
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
                    public Person(string firstName, string lastName, int age, bool isActive)
                    {
                        FirstName = firstName;
                        LastName = lastName;
                        Age = age;
                        IsActive = isActive;
                    }

                    public string FirstName { get; }
                    public string LastName { get; }
                    public int Age { get; }
                    public bool IsActive { get; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_RecordWithPrimaryConstructorAndAccessibleProperties() =>
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

                public record Product(string Id, string Name)
                {
                    public decimal Price { get; set; }
                    public int Quantity { get; init; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Constructor_MultipleAttributedConstructors_ShouldFail() =>
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
                    [DynamoMapperConstructor]
                    public Person(string name)
                    {
                        Name = name;
                        Age = 0;
                    }

                    [DynamoMapperConstructor]
                    public Person(string name, int age)
                    {
                        Name = name;
                        Age = age;
                    }

                    public string Name { get; }
                    public int Age { get; }
                }
                """,
                ExpectedDiagnosticId = "DM0103",
            },
            TestContext.Current.CancellationToken
        );
}
