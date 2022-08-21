namespace Iceberg.CommandLine.Commands;

public class ArgumentParseResult<T>
{
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ArgumentParseResult(T value) => Value = value;
    public ArgumentParseResult(string errorMessage) => ErrorMessage = errorMessage;
}