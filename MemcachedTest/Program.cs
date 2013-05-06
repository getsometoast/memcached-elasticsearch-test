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
            // local vm of elasticsearch:
            var elasticsearchConnection = "http://192.168.56.1:9200/";

            /*{"The value of the property 'type' cannot be parsed. The error is: The type 'Enyim.Caching.DefaultNodeLocator, Enyim.Caching' 
             * cannot be resolved. Please verify the spelling is correct or that the full type name is provided. 
             * (C:\\work\\memcached-elasticsearch-test\\MemcachedTest\\bin\\Debug\\MemcachedTest.vshost.exe.Config line 16)"}*/

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
            elasticClient.Index<Data>(data, new IndexParameters { Routing = data.UserId.ToString() });
        }
    }

    // some class representing the model
    public class Data
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public int TrackId { get; set; }
        public string Blah { get; set; }
    }
}
