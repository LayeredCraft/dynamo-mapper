using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Inheritance;

[DynamoMapper]
internal static partial class OrderMapper_Default
{
    internal static partial Dictionary<string, AttributeValue> ToItem(Order source);

    internal static partial Order FromItem(Dictionary<string, AttributeValue> item);
}

[DynamoMapper(IncludeBaseClassProperties = true)]
internal static partial class OrderMapper_WithBaseProps
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

internal static class Program
{
    private static void Main()
    {
        var order = new Order { Id = "order-123", Name = "Sample Order" };

        var itemDefault = OrderMapper_Default.ToItem(order);
        var itemWithBase = OrderMapper_WithBaseProps.ToItem(order);

        Console.WriteLine("Default mapper keys: " + string.Join(", ", itemDefault.Keys));
        Console.WriteLine("With base props keys: " + string.Join(", ", itemWithBase.Keys));

        var roundTripDefault = OrderMapper_Default.FromItem(itemWithBase);
        var roundTripWithBase = OrderMapper_WithBaseProps.FromItem(itemWithBase);

        Console.WriteLine("Default mapper Id: '" + roundTripDefault.Id + "'");
        Console.WriteLine("With base props Id: '" + roundTripWithBase.Id + "'");
    }
}
