using System;
using Microsoft.Extensions.Configuration;

namespace Svz.Common
{
    public class SvzConfig
    {
        public string Redis { get; set; }
        public SvzEsConfig ElasticSearch { get; set; }
        public SvzPostgresConfig Postgres { get; set; }


        public static SvzConfig GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = builder.Build();
            var svzConfig = config.GetSection("SvzConfig").Get<SvzConfig>();

            if (string.IsNullOrEmpty(svzConfig.Redis) ||
                svzConfig.ElasticSearch == null ||
                string.IsNullOrEmpty(svzConfig.ElasticSearch.Url) ||
                string.IsNullOrEmpty(svzConfig.ElasticSearch.Index) ||
                svzConfig.Postgres == null ||
                string.IsNullOrEmpty(svzConfig.Postgres.ConnectionString) ||
                string.IsNullOrEmpty(svzConfig.Postgres.Database))
                throw new Exception("Configuration is invalid!");

            svzConfig.ElasticSearch.Index = svzConfig.ElasticSearch.Index.ToLower();

            return svzConfig;
        }
    }

    public class SvzEsConfig
    {
        public string Url { get; set; }
        public string Index { get; set; }
    }

    public class SvzPostgresConfig
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string FullConnectionString => $"{ConnectionString};Database={Database}";
    }
}