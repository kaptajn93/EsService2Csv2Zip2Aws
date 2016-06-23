using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Es2Csv
{
    public interface IMapper<T> where T : class
    {
        string MapToCsv(List<IHit<object>> hits, Dictionary<string, string> mappings);
    }

    public class Mapper : IMapper<Object>
    {
        public string MapToCsv(List<IHit<object>> hits, Dictionary<string, string> mappings)
        {
            StringBuilder builder = new StringBuilder();

            var raw = hits.Select(x => x.Source).ToList();

            var csvFormat = string.Join(",", mappings.Values);
            builder.AppendLine(csvFormat);

            // foreach log/json row
            if (raw != null)
                foreach (var o in raw)
                {
                    Dictionary<string, string> csv = new Dictionary<string, string>();
                    var dict = JsonHelper.DeserializeAndFlatten(o.ToString());
                    var flat = JObject.FromObject(dict);

                    // for each property
                    foreach (var mappingFrom in mappings.Keys)
                    {
                        // check if the json object contains the property, select the token
                        JToken token;
                        flat.TryGetValue(mappingFrom, StringComparison.CurrentCultureIgnoreCase, out token);

                        if (token != null)
                        {
                            // if it does,..replace the old propertyname with the one defined in our mapping
                            string mappingTo;
                            if (mappings.TryGetValue(mappingFrom, out mappingTo))
                                csv.Add(mappingTo, $"\"{token.Value<string>()?.Trim()}\"");
                        }
                        else
                        {
                            string mappingTo;
                            if (mappings.TryGetValue(mappingFrom, out mappingTo))
                                csv.Add(mappingTo, $"\"{mappingFrom}\"");
                        }
                    }
                    builder.AppendLine(string.Join(",", csv.Values));
                }
            return builder.ToString();
        }
        public string WrapText(string text)
        {
            return $"\"{text}\",";
        }
    }
}
