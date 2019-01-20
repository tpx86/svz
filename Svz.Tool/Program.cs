using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CLAP;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redis;
using Svz.Common;

namespace Svz.Tool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Run<Program>(args);
        }

        [Verb]
        public static void Check()
        {
            try
            {
                CheckInternal().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static async Task CheckInternal()
        {
            var config = SvzConfig.GetConfig();

            Console.Write("Checking REDIS... ");
            using (var redisConn = ConnectionMultiplexer.Connect(config.Redis))
            {
                var db = redisConn.GetDatabase();
                var guid = Guid.NewGuid().ToString("N");
                var isOk = await db.StringSetAsync($"healthcheck-{guid}", "ok", TimeSpan.FromSeconds(15));
                if (!isOk) throw new Exception("Redis failed!");
            }

            Console.Write("OK!" + Environment.NewLine);

            Console.Write("Checking Postgres... ");
            using (var conn = new NpgsqlConnection(config.Postgres.ConnectionString))
            {
                var psqlTestVal = await conn.QuerySingleAsync<int>("SELECT 1");

                if (psqlTestVal != 1)
                    throw new Exception("Postgres failed!");
            }

            Console.Write("OK!" + Environment.NewLine);

            Console.Write("Checking ElasticSearch... ");
            var esConnection = new ConnectionSettings(new Uri(config.ElasticSearch.Url));
            var esClient = new ElasticClient(esConnection);
            var idxList = await esClient.GetIndexAsync("*");
            if (!idxList.IsValid) throw new Exception("ElasticSearch failed!");

            Console.Write("OK!" + Environment.NewLine);
        }

        [Verb]
        public static void Setup()
        {
            try
            {
                SetupInternal().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task SetupInternal()
        {
            var config = SvzConfig.GetConfig();

            using (var conn = new NpgsqlConnection(config.Postgres.ConnectionString))
            {
                var dbExists =
                    (await conn.QueryAsync<int>(
                        $"SELECT 1 from pg_database WHERE datname = '{config.Postgres.Database}'")).ToList();
                if (dbExists.Any())
                {
                    Ms("Dropping existing Svz database... ");
                    await conn.ExecuteScalarAsync($"DROP DATABASE \"{config.Postgres.Database}\"");
                    Ok();
                }

                Ms("Creating new Svz database... ");
                await conn.ExecuteScalarAsync($"CREATE DATABASE \"{config.Postgres.Database}\"");
                Ok();
            }

            Ms("Running migrations... ");
            var optBuild = new DbContextOptionsBuilder<BookDbContext>();
            optBuild.UseNpgsql($"{config.Postgres.ConnectionString};Database={config.Postgres.Database};");
            using (var ctx = new BookDbContext(optBuild.Options))
            {
                await ctx.Database.MigrateAsync();
                Ok();

                Ms("Import books from json... ");
                await ImportBooks("./books.json", ctx);
                Ok();
            }

            var esConnection = new ConnectionSettings(new Uri(config.ElasticSearch.Url));
            var esClient = new ElasticClient(esConnection);

            var idxResp = await esClient.GetIndexAsync(config.ElasticSearch.Index);
            foreach (var item in idxResp.Indices)
            {
                Ms($"Dropping ES Index: {item.Key}... ");
                var resp = await esClient.DeleteIndexAsync(item.Key);
                if (!resp.IsValid)
                    throw new Exception("Error deleting ES index: " + resp.ServerError);
                Ok();
            }

            Ms("Creating ES index... ");
            var respCreate = await esClient.CreateIndexAsync(config.ElasticSearch.Index);
            if (!respCreate.IsValid)
                throw new Exception("Error creating ES index: " + respCreate.ServerError);
            Ok();

            Ms("Indexing data... ");
            Mapper.Initialize(cfg => { cfg.CreateMap<Book, BookSearchView>(); });

            var bsearch = new List<BookSearchView>();
            using (var ctx = new BookDbContext(optBuild.Options))
            {
                var blist = ctx.Books.ToList();
                bsearch = Mapper.Map<List<Book>, List<BookSearchView>>(blist);
            }

            var bulkDesc = new BulkDescriptor();
            bulkDesc.CreateMany(bsearch, (x, y) => x.Id(y.BookId)).Index(config.ElasticSearch.Index);
            var respBulk = await esClient.BulkAsync(bulkDesc);
            if (!respBulk.IsValid)
                throw new Exception("Error indexing ES data: " + respBulk.ServerError);
            Ok();
        }

        private static void Ms(string msg)
        {
            Console.Write(msg);
        }

        private static void Ok()
        {
            Console.Write("OK!" + Environment.NewLine);
        }

        private static async Task ImportBooks(string filePath, BookDbContext ctx)
        {
            using (var jsonFileStream = new StreamReader(filePath))
            {
                jsonFileStream.ReadLine(); // Skip  "[" line
                string jsonLine = null;
                do
                {
                    jsonLine = jsonFileStream.ReadLine();
                    if (string.IsNullOrEmpty(jsonLine) || jsonLine == "]")
                        continue;

                    if (jsonLine[0] == ',')
                        jsonLine = jsonLine.Substring(1);

                    var bookData = JsonConvert.DeserializeObject<BookData>(jsonLine);

                    var book = new Book
                    {
                        Title = bookData.Title,
                        Isbn = bookData.Isbn.Replace("-", ""),
                        Ean = bookData.Ean.Replace("-", ""),
                        Category = bookData.Category,
                        Publisher = bookData.Publisher,
                        Authors = bookData.Authors,
                        IsActive = true,
                        IsInStock = true,
                        Description = bookData.Description.Length > 3000
                            ? bookData.Description.Substring(0, 3000)
                            : bookData.Description,
                        PagesCount = bookData.PagesCount ?? 0,
                        YearPublished = bookData.YearPublished ?? 0,
                        FormatWidthCm = (decimal) (bookData.FormatWidthCm ?? 0.0),
                        Cover = bookData.Cover
                    };

                    ctx.Books.Add(book);
                } while (!jsonFileStream.EndOfStream);
            }

            await ctx.SaveChangesAsync();
        }
        
        private class BookData
        {
            public string Title { get; set; }
            public string Isbn { get; set; }
            public string Ean { get; set; }
            public string Authors { get; set; }
            public string Publisher { get; set; }
            public short? PagesCount { get; set; }
            public short? YearPublished { get; set; }
            public float? FormatWidthCm { get; set; }
            public float? FormatHeightCm { get; set; }
            public decimal? PricePln { get; set; }
            public string Cover { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
        }
    }
}