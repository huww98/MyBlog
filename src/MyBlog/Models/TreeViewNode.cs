using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public class TreeViewData
    {
        public ICollection<TreeViewNode> RootNodes { get; } = new List<TreeViewNode>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(RootNodes, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        public static TreeViewData Build(IEnumerable<Category> allCategory, Category exception = null)
        {
            TreeViewData tree = new TreeViewData();
            var roots = allCategory.Where(c => c.ParentCategory == null);
            foreach (var root in roots)
            {
                if (root != exception)
                {
                    tree.RootNodes.Add(buildCategoryTreeNode(root));
                }
            }
            return tree;
        }

        private static TreeViewNode buildCategoryTreeNode(Category category, Category exception = null)
        {
            TreeViewNode nodeData = new TreeViewNode { Text = category.Name };
            nodeData.JsonExtensionData.Add("category_id", category.ID);
            foreach (var c in category.ChildCategories)
            {
                if (c != exception)
                {
                    if (nodeData.Nodes==null)
                    {
                        nodeData.Nodes = new List<TreeViewNode>();
                    }
                    nodeData.Nodes.Add(buildCategoryTreeNode(c, exception));
                }
            }
            return nodeData;
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

        [JsonExtensionData]
        public IDictionary<string, object> JsonExtensionData { get; set; } = new Dictionary<string, object>();

    }
}
