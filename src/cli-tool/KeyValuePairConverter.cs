using System.Text.RegularExpressions;
using Vertical.Cli.Conversion;

namespace Vertical.Migrate.Cli;

public sealed partial class KeyValuePairConverter : ValueConverter<KeyValuePair<string, string>>
{
    /// <inheritdoc />
    public override KeyValuePair<string, string> Convert(string s)
    {
        var patternMatch = MyRegex().Match(s);
        if (!patternMatch.Success)
        {
            throw new FormatException($"Invalid key/value pair format: '{s}'");
        }

        return new KeyValuePair<string, string>(
            patternMatch.Groups[1].Value,
            patternMatch.Groups[2].Value);
    }

    [GeneratedRegex("([^=]+)=(.+)")]
    private static partial Regex MyRegex();
}