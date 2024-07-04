namespace LiteApi;

internal class LiteApiConfiguration
{
    public MimeType[] MimeTypes { get; set; } = null!;

    public Lazy<Dictionary<string, MimeType>> MimeTypesByExtension;

    public LiteApiConfiguration()
    {
        MimeTypesByExtension = new Lazy<Dictionary<string, MimeType>>(() =>
        {
            var dictionary = new Dictionary<string, MimeType>();
            foreach (var mimeType in MimeTypes)
            {
                dictionary.Add(mimeType.Extension, mimeType);
            }
            return dictionary;
        });
    }
}

internal class MimeType
{
    public required string Extension { get; set; }
    public required string Type { get; set; }
}