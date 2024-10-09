using System.Text.RegularExpressions;

namespace Vertical.Migrate;

public record SourceParsingDialect(
    Regex IdMatchProvider,
    Func<string, bool> MigrationSectionPredicate,
    Func<string, bool> RollbackSectionPredicate,
    Func<string, bool> BeginTransactionPredicate,
    Func<string, bool> CommitTransactionPredicate,
    Func<string, bool> CommentPredicate);