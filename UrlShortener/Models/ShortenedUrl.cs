namespace UrlShortener.Models
{
    public sealed class ShortenedUrl
    {
        public long Id { get; init; }
        public string OriginalUrl { get; init; } = string.Empty;
        public string ShortCode { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
