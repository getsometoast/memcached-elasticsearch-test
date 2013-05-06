using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var elasticsearchConnection = "http://192.168.56.1:9200/";
            var memcachedConnection = "http://10.211.55.10:11211/";
            var rabbitMqConnection = "http://127.0.0.1:54325/";

            var blah = new Enyim.Caching.Configuration.MemcachedClientConfiguration();
            blah.AddServer(memcachedConnection);

            
        }
    }
}
