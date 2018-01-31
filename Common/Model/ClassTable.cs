using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Model
{
    public class ClassTable
    {
        [JsonProperty("Courselist")]
        public List<Course> ClassTableItems { get; set; }

        [JsonProperty("Count")]
        public int Count { get; set; }
    }
}
