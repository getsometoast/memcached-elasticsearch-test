using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nest;
using Newtonsoft.Json;

namespace MemcachedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var elasticsearchConnection = "http://192.168.56.1:9200/";
            var memcachedConnection = "http://10.211.55.10:11211/";
            //var rabbitMqConnection = "http://127.0.0.1:54325/";

            var blah = new Enyim.Caching.Configuration.MemcachedClientConfiguration();
            blah.AddServer(memcachedConnection);

            // setup memcached
            // setup elasticsearch
            var connection = new ConnectionSettings(new Uri(elasticsearchConnection));
            var client = new ElasticClient(connection);

            // read the data
            var jsonData = File.ReadAllText("Data/test-data.json");
            var data = JsonConvert.DeserializeObject<Data>(jsonData);

            // check the alias exists in memcached
            var aliasExists = false;
            AliasParams alias = null;

            // if not:
            if (!aliasExists)
            {
                
                // create the alias
                alias = new AliasParams
                {
                    Alias = data.UserId.ToString(),
                    Filter = data.UserId.ToString(),
                    Index = "the-index",
                    Routing = data.UserId.ToString()
                };

                client.Alias(alias);

                // add the alias to memcached


            }

            // insert data into elasticsearch via the alias
            client.Index<Data>(data, new IndexParameters { Routing = alias.Routing }); // against the alias - how in NEST?!!
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
}
