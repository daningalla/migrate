using Microsoft.Extensions.Logging;

namespace Vertical.Migrate;

public record DatabaseProviderFactory(string ProviderId, Func<ILoggerFactory, Task<IDatabaseProvider>> AsyncFactory);