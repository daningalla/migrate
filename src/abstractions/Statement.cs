namespace Vertical.Migrate;

/// <summary>
/// Represents a statement.
/// </summary>
/// <param name="Type">Statement type.</param>
/// <param name="Body">Body of the statement (N/A for transaction types).</param>
public record Statement(StatementType Type, string Body);