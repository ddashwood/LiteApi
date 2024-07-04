
namespace LiteApi.Files;

internal class FileHelper : IFileHelper
{
    public bool FileExists(string path) => File.Exists(path);

    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken) => 
        await File.ReadAllBytesAsync(path, cancellationToken);

}
