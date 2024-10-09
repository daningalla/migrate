namespace Vertical.Migrate;

/// <summary>
/// Defines the different types of statements in a migration section.
/// </summary>
public enum StatementType
{
    StatementBody,
    BeginTransaction,
    CommitTransaction
}