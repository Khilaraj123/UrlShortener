using Dapper;
using UrlShortener.Interfaces;
using UrlShortener.Models;

namespace UrlShortener.Repositories
{
    public sealed class UrlRepository(IDbConnectionFactory connectionFactory) : IUrlRepository
    {
        public async Task<long> GetNextIdAsync()
        {
            await using var connection = connectionFactory.CreateConnection();

            const string sql = "SELECT nextval(pg_get_serial_sequence('urls', 'id'));";

            return await connection.ExecuteScalarAsync<long>(sql);
        }

        public async Task CreateAsync(ShortenedUrl shortenedUrl)
        {
            await using var connection = connectionFactory.CreateConnection();

            const string sql = """
            INSERT INTO Urls
                (Id, OriginalUrl, ShortCode, CreatedAt)
            OVERRIDING SYSTEM VALUE
            VALUES
                (@Id, @OriginalUrl, @ShortCode, @CreatedAt);
            """;

            await connection.ExecuteAsync(sql, new
            {
                shortenedUrl.Id,
                shortenedUrl.OriginalUrl,
                shortenedUrl.ShortCode,
                shortenedUrl.CreatedAt
            });
        }

        public async Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode)
        {
            await using var connection = connectionFactory.CreateConnection();

            const string sql = """
            SELECT
                Id,
                OriginalUrl,
                ShortCode,
                CreatedAt
            FROM Urls
            WHERE ShortCode = @ShortCode;
            """;

            return await connection.QuerySingleOrDefaultAsync<ShortenedUrl>(
                sql,
                new { ShortCode = shortCode });
        }
    }
}
