using DynamoMapper.Runtime;
using Humanizer;

namespace DynamoMapper.Extensions;

internal static class DynamoNamingConventionExtensions
{
    extension(DynamoNamingConvention dynamoNamingConvention)
    {
        internal Func<string, string> GetConverter() =>
            dynamoNamingConvention switch
            {
                DynamoNamingConvention.Exact => s => s,
                DynamoNamingConvention.CamelCase => InflectorExtensions.Camelize,
                DynamoNamingConvention.PascalCase => InflectorExtensions.Pascalize,
                DynamoNamingConvention.SnakeCase => InflectorExtensions.Underscore,
                DynamoNamingConvention.KebabCase => InflectorExtensions.Kebaberize,
                _ => throw new InvalidOperationException(
                    $"Unknown {nameof(DynamoNamingConvention)} of {dynamoNamingConvention}"
                ),
            };
    }
}
