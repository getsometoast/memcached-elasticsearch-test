﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nest;
using Newtonsoft.Json;
using Enyim.Caching.Memcached;
using Enyim.Caching;

namespace MemcachedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var elasticsearchConnection = "http://192.168.56.1:9200/";
            //var memcachedConnection = "http://10.211.55.10:11211/"; - done in app.config
            //var rabbitMqConnection = "http://127.0.0.1:54325/"; - not using for this test...

            // setup memcached
            var memClient = new MemcachedClient();

            // setup elasticsearch
            var connection = new ConnectionSettings(new Uri(elasticsearchConnection));
            var elasticClient = new ElasticClient(connection);

            // read the data
            var jsonData = File.ReadAllText("Data/test-data.json");
            var data = JsonConvert.DeserializeObject<Data>(jsonData);

            // check the alias exists in memcached
            var aliasExists = memClient.Get(data.UserId.ToString()) as bool?;
            AliasParams alias = null;

            // if not:
            if (!aliasExists.HasValue)
            {
                
                // create the alias
                alias = new AliasParams
                {
                    Alias = data.UserId.ToString(),
                    Filter = data.UserId.ToString(),
                    Index = "the-index",
                    Routing = data.UserId.ToString()
                };

                elasticClient.Alias(alias);

                // add the alias to memcached
                memClient.Store(StoreMode.Set, alias.Alias, true);
            }

            // insert data into elasticsearch via the alias
            elasticClient.Index<Data>(data, new IndexParameters { Routing = alias.Routing }); // against the alias - how in NEST?!!
        }
    }

    // some class representing the model
    private class Data
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public int TrackId { get; set; }
        public string Blah { get; set; }
    }

    private class AliasTranscoder : ITranscoder
    {
        public object Deserialize(CacheItem item)
        {
            Console.WriteLine(item.Data.Count);

            return null;
        }

        public CacheItem Serialize(object value)
        {
            Console.WriteLine(value.ToString());

            return new CacheItem();
        }
    }
}
