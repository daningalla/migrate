using System.Text;

namespace Vertical.Migrate.Engine;

public sealed class StatementListBuilder
{
    private readonly StringBuilder _buffer = new();
    private bool _inTransactionBlock;

    public List<Statement> Statements { get; } = new();

    public bool SectionEntered { get; set; }

    public void TryEnterTransactionBlock(string path, int lineNumber)
    {
        if (_inTransactionBlock)
        {
            throw TransactionError(path, lineNumber, "nested transactions not allowed.");
        }

        TryFlushStatement();
        Statements.Add(new Statement(StatementType.BeginTransaction, string.Empty));
        _inTransactionBlock = true;
    }

    public void TryLeaveTransactionBlock(string path, int lineNumber)
    {
        if (!_inTransactionBlock)
        {
            throw TransactionError(path, lineNumber, "not currently positioned within a transaction block.");
        }

        TryFlushStatement();
        Statements.Add(new Statement(StatementType.CommitTransaction, string.Empty));
        _inTransactionBlock = false;
    }

    private static Exception TransactionError(string path, int lineNumber, string error)
    {
        throw new InvalidOperationException($"Error in source {path} @line {lineNumber}: {error}");
    }

    public void AppendStatements(string content) => _buffer.AppendLine(content);


    public Statement[] Build()
    {
        TryFlushStatement();
        return Statements.ToArray();
    }

    private void TryFlushStatement()
    {
        if (_buffer.Length == 0)
            return;
            
        Statements.Add(new Statement(StatementType.StatementBody, _buffer.ToString()));
        _buffer.Clear();
    }
}