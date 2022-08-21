using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

public interface IOptionParser<T>
{
    string[] Aliases { get; }
    string Description { get; }
    bool AllowMultipleArgumentsPerToken { get; }
    bool IsRequired { get; }

    ArgumentParseResult<T> Parse(IEnumerable<string> inputTokens);
    Option<T> CreateOption();
}
