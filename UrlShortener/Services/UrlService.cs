using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Npgsql;
using System.Data;
using UrlShortener.Interfaces;
using UrlShortener.Models;
using UrlShortener.Utilities;

namespace UrlShortener.Services
{
    public class UrlService : IUrlService
    {
        private readonly string _postgresConnectionString;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UrlService> _logger;

        public UrlService(IConfiguration configuration, IDistributedCache cache, ILogger<UrlService> logger)
        {
            _postgresConnectionString = configuration.GetConnectionString("Postgres")!;
            _cache = cache;
            _logger = logger;
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_postgresConnectionString);

        public async Task<string?> GetOriginalUrlAsync(string shortCode)
        {
            var cachedUrl = await _cache.GetStringAsync(shortCode);
            if (!string.IsNullOrEmpty(cachedUrl))
            {
                _logger.LogInformation("Cache HIT for short code: {ShortCode}", shortCode);
                return cachedUrl;
            }

            _logger.LogWarning("Cache MISS for short code: {ShortCode}. Querying PostgreSQL...", shortCode);

            using var connection = CreateConnection();

            const string sql = "SELECT OriginalUrl FROM Urls WHERE ShortCode = @ShortCode;";
            var originalUrl = await connection.QueryFirstOrDefaultAsync<string>(sql, new
            {
                ShortCode = shortCode
            });

            if (!string.IsNullOrEmpty(originalUrl))
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(1)
                };
                await _cache.SetStringAsync(shortCode, originalUrl, cacheOptions);
            }
            return originalUrl;
        }

        public async Task<ShortenedUrl> ShortenUrlAsync(string originalUrl)
        {
            using var connection = CreateConnection();

            const string insertSql = @"
                INSERT INTO Urls (OriginalUrl, ShortCode, CreatedAt)
                VALUES (@OriginalUrl, '', @CreatedAt) RETURNING ID;
            ";

            var id = await connection.ExecuteScalarAsync<Guid>(insertSql, new
            {
                OriginalUrl = originalUrl,
                CreatedAt = DateTime.UtcNow
            });

            var shortCode = Base62Converter.Encode(id);

            const string updateSql = "UPDATE Urls SET ShortCode = @ShortCode WHERE Id = @Id";
            await connection.ExecuteAsync(updateSql, new
            {
                ShortCode = shortCode,
                Id = id
            });

            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromDays(1)
            };
            await _cache.SetStringAsync(shortCode, originalUrl, cacheOptions);

            return new ShortenedUrl
            {
                Id = id,
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
