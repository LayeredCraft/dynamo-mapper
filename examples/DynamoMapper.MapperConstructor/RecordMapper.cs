using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.MapperConstructor;

[DynamoMapper]
public static partial class PersonRecordMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(PersonRecord personRecord);

    public static partial PersonRecord FromItem(Dictionary<string, AttributeValue> item);

    // public static void Do()
    // {
    //     var person = new PersonRecord
    //     {
    //         FirstName = "John",
    //         LastName = "Doe",
    //         Age = 30,
    //     };
    // }
}

public record PersonRecord(string FirstName, string LastName, int Age);
