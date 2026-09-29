namespace UrlShortener.DTOs
{
    public record ShortenUrlResponse
    (
        string ShortUrl,
        string ShortCode,
        string OriginalUrl
    );
}
