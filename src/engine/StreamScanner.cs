namespace Vertical.Migrate.Engine;

public sealed class Scan<TState, TResult>
{
    public Scan(TState state, TResult result)
    {
        State = state;
        Result = result;
    }

    public TState State { get; }
    
    public TResult Result { get; private set; }
    
    public bool Completed { get; private set; }
    
    public int LineNumber { get; set; }

    public Scan<TState, TResult> Next() => this;

    public Scan<TState, TResult> Next(TResult result)
    {
        Result = result;
        return this;
    }

    public Scan<TState, TResult> Complete()
    {
        Completed = true;
        return this;
    }

    public Scan<TState, TResult> Complete(TResult result)
    {
        Result = result;
        Completed = true;
        return this;
    }
}

public static class StreamScanner
{
    public static async Task<TResult> ScanAsync<TState, TResult>(
        Func<Stream> provider,
        TState state,
        TResult seed,
        Func<Scan<TState, TResult>, string, Scan<TState, TResult>> evaluator,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(provider());
        var scan = new Scan<TState, TResult>(state, seed);
        
        while (true)
        {
            var content = await reader.ReadLineAsync(cancellationToken);
            if (content == null)
            {
                return scan.Result;
            }

            scan.LineNumber++;
            scan = evaluator(scan, content);

            if (!scan.Completed)
                continue;

            return scan.Result;
        }
    }
}