using System.Runtime.CompilerServices;

namespace LayeredCraft.DynamoMapper.Generators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
