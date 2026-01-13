namespace DynamoMapper.Runtime;

// ReSharper disable InconsistentNaming
/// <summary>DynamoDB AttributeValue kind/type identifiers.</summary>
public enum DynamoKind
{
    /// <summary>String attribute (S).</summary>
    S,

    /// <summary>Number attribute (N).</summary>
    N,

    /// <summary>Boolean attribute (BOOL).</summary>
    BOOL,

    // Future Phase 2+ types:
    /// <summary>Binary attribute (B).</summary>
    B,

    /// <summary>Map attribute (M).</summary>
    M,

    /// <summary>List attribute (L).</summary>
    L,

    /// <summary>Null attribute (NULL).</summary>
    NULL,

    /// <summary>String Set attribute (SS).</summary>
    SS,

    /// <summary>Number Set attribute (NS).</summary>
    NS,

    /// <summary>Binary Set attribute (BS).</summary>
    BS,
}
