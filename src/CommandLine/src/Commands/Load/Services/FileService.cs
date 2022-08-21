namespace Iceberg.CommandLine.Commands.Load.Services;

public interface IFileService
{
    bool Exists(string filePath);
}
public class FileService : IFileService
{
    public bool Exists(string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        return fileInfo.Exists;
    }
}
