using Newtonsoft.Json;
using System.Collections.Generic;

namespace MyBlog.Models
{
    public class CategorySelectorViewModel
    {
        public string ButtonClasses { get; set; }
        public string Name { get; set; }
        public bool IsMultiple { get; set; }
        public ICollection<Category> PreselectedCategories { get; set; }
        public ICollection<TreeViewNode> RootNodes { get; } = new List<TreeViewNode>();

        public string GetJsonData()
            => JsonConvert.SerializeObject(RootNodes, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
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
