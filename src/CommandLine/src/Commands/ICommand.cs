using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

internal interface ICommand
{
   Command Value { get; }
}
