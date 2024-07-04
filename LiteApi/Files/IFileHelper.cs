namespace LiteApi.Files;

internal interface IFileHelper
{
    bool FileExists(string path);
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken);
}
