using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.ViewComponents
{
    public class CategorySelectorViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategorySelectorViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string name,
            string buttonClasses = "btn btn-default",
            bool isMultiple = false,
            ICollection<int> preselectedIDs = null,
            int exceptionID = -1)
        {
            CategorySelectorViewModel viewModel = new CategorySelectorViewModel { ButtonClasses = buttonClasses, Name = name, IsMultiple = isMultiple };
            await _context.Category.LoadAsync();
            if (preselectedIDs != null)
            {
                viewModel.PreselectedCategories = _context.Category.Local.Where(c => preselectedIDs.Contains(c.ID)).ToList();
            }

            var roots = _context.Category.Local.Where(c => c.ParentCategory == null);
            foreach (var root in roots)
            {
                if (root.ID != exceptionID)
                {
                    viewModel.RootNodes.Add(buildCategoryTreeNode(root, preselectedIDs, exceptionID));
                }
            }
            return View(viewModel);
        }

        private static TreeViewNode buildCategoryTreeNode(
            Category category,
            ICollection<int> preselectedIDs = null,
            int exceptionID = -1)
        {
            TreeViewNode nodeData = new TreeViewNode { Text = category.Name };
            nodeData.JsonExtensionData.Add("category_id", category.ID);
            if (preselectedIDs?.Contains(category.ID) == true)
            {
                if (nodeData.State == null)
                {
                    nodeData.State = new TreeViewNodeState();
                }
                nodeData.State.Selected = true;
            }
            foreach (var c in category.ChildCategories)
            {
                if (c.ID != exceptionID)
                {
                    if (nodeData.Nodes == null)
                    {
                        nodeData.Nodes = new List<TreeViewNode>();
                    }
                    nodeData.Nodes.Add(buildCategoryTreeNode(c, preselectedIDs, exceptionID));
                }
            }
            return nodeData;
        }
    }
}