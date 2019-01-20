using Dapper;
using Npgsql;

namespace Svz.Common
{
    public class BookCacheValueSource : ICacheValueSource<int, BookView>
    {
        private readonly SvzConfig _config;

        public BookCacheValueSource(SvzConfig config)
        {
            _config = config;
        }

        public BookView Get(int key)
        {
            var bookQuery = "SELECT * FROM \"Book\" WHERE \"BookId\" = @BookId";
            using (var conn = new NpgsqlConnection(_config.Postgres.FullConnectionString))
            {
                return conn.QuerySingle<BookView>(bookQuery, new {BookId = key});
            }
        }
    }
}