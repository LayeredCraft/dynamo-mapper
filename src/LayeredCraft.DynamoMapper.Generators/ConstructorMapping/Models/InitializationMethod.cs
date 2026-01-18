namespace DynamoMapper.Generator.ConstructorMapping.Models;

/// <summary>Specifies how a property should be initialized during object construction.</summary>
internal enum InitializationMethod
{
    /// <summary>Property is passed as a constructor parameter.</summary>
    ConstructorParameter,

    /// <summary>Property is initialized using object initializer syntax (init-only properties).</summary>
    InitSyntax,

    /// <summary>Property is assigned after construction (settable properties).</summary>
    PostConstruction,
}
