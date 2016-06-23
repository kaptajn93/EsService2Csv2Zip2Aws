using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace Es2Csv
{
    public class Constants
    {
        public class AnalyzerNames
        {
            public const string NgramAnalyzer = "ngramAnalyzer";
            public const string Keyword = "keyword";
            public const string Lowercase = "lowercase";
        }
        public class TokenizerNames
        {
            public const string NoWhitespaceNGram = "noWhitespaceNGram";
        }
        public class IndexNames
        {
            public const string Da = "da";
        }
    }

    public class ElasticManager
    {
        public Uri NodeUri { get; set; }
        public ConnectionSettings Settings { get; set; }
        public ElasticClient Client { get; set; }

        public List<IHit<object>> EntrySearch(int from, int size, string index, string type, Dictionary<string, string> mappings, string sortBy)
        {            
            Settings = new ConnectionSettings(NodeUri);
            Client = new ElasticClient(Settings);
            var hits = new List<IHit<object>>();
            var response = Client.Search<Object>(e => e
               .Index(index)
               .Type(type)
               .From(from)
               .Size(10000)
               .Scroll("2m")
               .Sort(s => s.Field(f => f.Field(sortBy)))
           );

            hits.AddRange(response.Hits);
            Scroll(response.ScrollId, ref hits);

            return hits;
        }

        private void Scroll(string scrollId, ref List<IHit<object>> hits)
        {
            Console.WriteLine("Start scroll using id: " + scrollId + " ->  current count: " + hits.Count);

            if (!string.IsNullOrWhiteSpace(scrollId))
            {
                var scrollResponse = Client.Scroll<object>("4s", scrollId);
                if (scrollResponse.Hits.Any())
                {
                    hits.AddRange(scrollResponse.Hits);
                    Console.WriteLine($"Added {hits.Count} documents");

                    if (!string.IsNullOrEmpty(scrollResponse.ScrollId))
                        Scroll(scrollId, ref hits);
                }
            }

        }
    }
}