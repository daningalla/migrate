namespace Vertical.Migrate.Engine;

internal static class Async
{
    public static async Task<T> Immediate<T>(Func<Task<T>> fn) => await fn();
}