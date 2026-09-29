using UrlShortener.Models;

namespace UrlShortener.Interfaces
{
    public interface IUrlRepository
    {
        Task<long> GetNextIdAsync();

        Task CreateAsync(ShortenedUrl shortenedUrl);

        Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode);
    }
}
