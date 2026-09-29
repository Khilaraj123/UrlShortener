using UrlShortener.Models;

namespace UrlShortener.Interfaces
{
    public interface IUrlService
    {
        Task<ShortenedUrl> ShortenUrlAsync(string originalUrl);
        Task<string?> GetOriginalUrlAsync(string shortCode);
    }
}
