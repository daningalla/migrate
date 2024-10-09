namespace Vertical.Migrate;

public static class ParseHelper
{
    public static T ParseOrThrow<T>(string propertyName, string value) where T : IParsable<T>
    {
        if (T.TryParse(value, provider: null, out var parsedValue))
            return parsedValue;

        throw new FormatException($"Value '{value}' is not a valid {typeof(T)} value for property {propertyName}.");
    }
}