namespace INTERNAL_SOURCE_LOAD.Models
{
    /// <summary>
    /// Represents a train with associated identifiers.
    /// </summary>
    /// <param name="G">Group / category of the train.</param>
    /// <param name="L">Line the train operates on. Can be null if not specified.</param>
    [TableName("Trains")]
    public record Train(string G, string? L);
}
