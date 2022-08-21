using System.CommandLine;
using System.Diagnostics;

namespace Iceberg.CommandLine.Commands;

public abstract class BaseOptionParser<T> : IOptionParser<T>
{
    public abstract string[] Aliases { get; }
    public abstract string Description { get; }
    public abstract bool AllowMultipleArgumentsPerToken { get; }
    public abstract bool IsRequired { get; }

    public abstract ArgumentParseResult<T> Parse(IEnumerable<string> inputTokens);

    public virtual Option<T> CreateOption()
    {
        return new Option<T>(
            aliases: Aliases,
            description: Description,
            parseArgument: result =>
            {
                var tokens = result.Tokens.Select(x => x.Value);
                var parseResult = Parse(tokens);

                if (parseResult.HasError)
                {
                    result.ErrorMessage = parseResult.ErrorMessage;
                    return default!;
                }

                Debug.Assert(parseResult.Value != null);
                return parseResult.Value;
            })
        {
            AllowMultipleArgumentsPerToken = AllowMultipleArgumentsPerToken,
            IsRequired = IsRequired
        };
    }

    protected ArgumentParseResult<T> Success(T value) => new ArgumentParseResult<T>(value);

    protected ArgumentParseResult<T> Failure(string errorMessage) => new ArgumentParseResult<T>(errorMessage);
}
