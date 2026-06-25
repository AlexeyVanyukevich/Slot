namespace UBP.OpenApi;

public sealed class OpenApiDocumentOptions
{
    internal string DocumentName { get; set; } = "v1";
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public string? Description { get; set; }
}
