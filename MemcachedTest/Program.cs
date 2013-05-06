using System;
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
            // setup elasticsearch
            var connection = new ConnectionSettings(new Uri("http://192.168.56.1:9200/"));
            connection.SetDefaultIndex("the-index");
            var elasticClient = new ElasticClient(connection);

            // read the data
            var jsonData = File.ReadAllText("Data/test-data.json");
            var data = JsonConvert.DeserializeObject<Data>(jsonData);

            using (var memClient = new MemcachedClient())
            {
                // check the alias exists in memcached
                var aliasExists = memClient.Get(data.userId.ToString()) as bool?;

                AliasParams alias = null;

                // if not:
                if (!aliasExists.HasValue)
                {

                    // create the alias
                    alias = new AliasParams
                    {
                        Alias = data.userId.ToString(),
                        Filter = data.userId.ToString(),
                        Index = "the-index",
                        Routing = data.userId.ToString()
                    };

                    elasticClient.Alias(alias);

                    // add the alias to memcached
                    memClient.Store(StoreMode.Set, alias.Alias, true);
                }
            }

            // insert data into elasticsearch via the alias
            elasticClient.Index<Data>(data, new IndexParameters { Routing = data.userId.ToString() });
        }
    }

    // some class representing the model
    public class Data
    {
        public string id { get; set; }
        public int userId { get; set; }
        public int trackId { get; set; }
        public string blah { get; set; }
    }
}
