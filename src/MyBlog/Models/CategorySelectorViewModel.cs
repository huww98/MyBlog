using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public class CategorySelectorViewModel
    {
        public string Name { get; set; }

        public ICollection<TreeViewNode> RootNodes { get; } = new List<TreeViewNode>();

        public string GetJsonData()
        {
            return JsonConvert.SerializeObject(RootNodes, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }
    }

    public class TreeViewNode
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("nodes")]
        public ICollection<TreeViewNode> Nodes { get; set; }

        [JsonProperty("tags")]
        public ICollection<string> Tags { get; set; }

        [JsonProperty("state")]
        public TreeViewNodeState State { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> JsonExtensionData { get; set; } = new Dictionary<string, object>();

    }

    public class TreeViewNodeState
    {
        [JsonProperty("checked")]
        public bool Checked { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("expanded")]
        public bool Expanded { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}
