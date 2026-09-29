using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Interfaces;
using UrlShortener.Models;
using UrlShortener.Utilities;

namespace UrlShortener.Services
{
    public class UrlService : IUrlService
    {
        private const string NotFoundSentinel = "__NOT_FOUND__";

        private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7),
            SlidingExpiration = TimeSpan.FromDays(1)
        };

        private static readonly DistributedCacheEntryOptions NotFoundCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };

        private readonly IUrlRepository _urlRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UrlService> _logger;

        public UrlService(IUrlRepository urlRepository, IDistributedCache cache, ILogger<UrlService> logger)
        {
            _urlRepository = urlRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string?> GetOriginalUrlAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode) || !Base62Converter.IsValid(shortCode))
            {
                return null;
            }

            var cachedUrl = await _cache.GetStringAsync(shortCode);
            if (!string.IsNullOrEmpty(cachedUrl))
            {
                if (cachedUrl == NotFoundSentinel)
                {
                    return null;
                }

                _logger.LogInformation("Cache HIT for short code: {ShortCode}", shortCode);
                return cachedUrl;
            }

            _logger.LogWarning("Cache MISS for short code: {ShortCode}. Querying PostgreSQL...", shortCode);

            var url = await _urlRepository.GetByShortCodeAsync(shortCode);

            if (url is null)
            {
                await _cache.SetStringAsync(shortCode, NotFoundSentinel, NotFoundCacheOptions);
                return null;
            }

            await _cache.SetStringAsync(
                shortCode,
                url.OriginalUrl,
                DefaultCacheOptions);

            return url.OriginalUrl;
        }

        public async Task<ShortenedUrl> ShortenUrlAsync(string originalUrl)
        {
            var createdAt = DateTime.UtcNow;

            var id = await _urlRepository.GetNextIdAsync();
            var shortCode = Base62Converter.Encode(id);

            var shortenedUrl = new ShortenedUrl
            {
                Id = id,
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = createdAt
            };

            await _urlRepository.CreateAsync(shortenedUrl);

            await _cache.SetStringAsync(shortCode, originalUrl, DefaultCacheOptions);

            return shortenedUrl;
        }
    }
}
