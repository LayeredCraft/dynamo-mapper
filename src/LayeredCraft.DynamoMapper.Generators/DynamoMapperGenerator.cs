using Microsoft.CodeAnalysis;

namespace DynamoMapper;

[Generator]
public class DynamoMapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        throw new NotImplementedException();
}
