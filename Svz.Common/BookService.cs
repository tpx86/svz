using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Svz.Common
{
    public class BookService
    {
        private readonly ICacheManager<int, BookView> _cache;
        private readonly SvzConfig _config;

        public BookService()
        {
            _config = SvzConfig.GetConfig();
            _cache = new CacheManager<int, BookView>(new BookCacheValueSource(_config),
                new RedisCacheProvider<int, BookView>(_config, "Svz", 15));
        }

        private BookDbContext GetDbContext()
        {
            var optBuild = new DbContextOptionsBuilder<BookDbContext>();
            optBuild.UseNpgsql($"{_config.Postgres.ConnectionString};Database={_config.Postgres.Database};");
            return new BookDbContext(optBuild.Options);
        }

        private ElasticClient GetEsClient()
        {
            var esConnection = new ConnectionSettings(new Uri(_config.ElasticSearch.Url));
            return new ElasticClient(esConnection);
        }

        public async Task<IList<BookView>> GetBooks()
        {
            using (var ctx = GetDbContext())
            {
                var raw = await ctx.Books.OrderBy(x => x.Title).ToListAsync();
                return Mapper.Map<List<Book>, List<BookView>>(raw);
            }
        }

        public CacheResponse<BookView> GetById(int bookId)
        {
            return _cache.Get(bookId);
        }

        public async Task<IList<BookView>> Search(string query)
        {
            var client = GetEsClient();

            var res = await client.SearchAsync<BookSearchView>(s => s.Size(10).Index(_config.ElasticSearch.Index).Query(
                q => q.MultiMatch(mm =>
                    mm.Query(query)
                        .Type(TextQueryType.MostFields)
                        .Fields(
                            f => f
                                .Field(ff => ff.Title)
                                .Field(ff => ff.Description)
                        ))));

            return Mapper.Map<List<BookSearchView>, List<BookView>>(res.Documents.ToList());
        }
    }
}