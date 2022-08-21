using System.Diagnostics;

namespace Iceberg.CommandLine.Commands.View.Services;

// TODO: Do better.
internal class WindowsProcessService : IProcessService
{
    public void OpenFileWithDefaultProgram(string filePath)
    {
        using var fileOpener = new Process();
        fileOpener.StartInfo.FileName = "explorer";
        fileOpener.StartInfo.Arguments = $"\"{filePath}\"";

        fileOpener.Start();
    }
}
