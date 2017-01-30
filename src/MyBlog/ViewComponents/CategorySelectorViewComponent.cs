using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Data;
using MyBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace MyBlog.ViewComponents
{
    public class CategorySelectorViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategorySelectorViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string name, bool isMultiple, ICollection<Category> preselected=null, Category exception = null)
        {
            CategorySelectorViewModel viewModel = new CategorySelectorViewModel();
            viewModel.Name = name;
            await _context.Category.LoadAsync();
            var roots = _context.Category.Local.Where(c => c.ParentCategory == null);
            foreach (var root in roots)
            {
                if (root != exception)
                {
                    viewModel.RootNodes.Add(buildCategoryTreeNode(root, preselected, exception));
                }
            }
            return View(viewModel);
        }

        private static TreeViewNode buildCategoryTreeNode(Category category, ICollection<Category> preselected=null, Category exception = null)
        {
            TreeViewNode nodeData = new TreeViewNode { Text = category.Name };
            nodeData.JsonExtensionData.Add("category_id", category.ID);
            if (preselected?.Contains(category)==true)
            {
                if (nodeData.State == null)
                {
                    nodeData.State = new TreeViewNodeState();
                }
                nodeData.State.Selected = true;
            }
            foreach (var c in category.ChildCategories)
            {
                if (c != exception)
                {
                    if (nodeData.Nodes == null)
                    {
                        nodeData.Nodes = new List<TreeViewNode>();
                    }
                    nodeData.Nodes.Add(buildCategoryTreeNode(c, preselected, exception));
                }
            }
            return nodeData;
        }
    }
}
