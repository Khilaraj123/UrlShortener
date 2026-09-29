using Npgsql;

namespace UrlShortener.Interfaces
{
    public interface IDbConnectionFactory
    {
        NpgsqlConnection CreateConnection();
    }
}
