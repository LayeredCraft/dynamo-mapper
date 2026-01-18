using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.MapperConstructor;

[DynamoMapper]
public static partial class PersonClassMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(PersonClass personClass);

    public static partial PersonClass FromItem(Dictionary<string, AttributeValue> item);
}

public class PersonClass
{
    [DynamoMapperConstructor]
    public PersonClass(string firstName, string lastName, int age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public int Age { get; }
}
