using Cust.Data;
using Cust.Models;
using Cust.Models.BlogViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace Cust.Pages.Articles
{
    public class ArticleTagsPageModel : PageModel
    {
        public List<AssignedTagData> AssignedTagDataList;

        public void PopulateAssignedTagData(CustContext context,
                                       Article article)
        {
            var allTags = context.Tags.Where(t => !t.Disabled);
            var articleTags = new HashSet<string>(
                article.Tags.Select(t => t.Id));

            AssignedTagDataList = new List<AssignedTagData>();
            //AssignedCourseDataList = new List<AssignedCourseData>();
            foreach (var tag in allTags)
            {
                AssignedTagDataList.Add(new AssignedTagData
                {
                    TagId = tag.Id,
                    //Title = course.Title,
                    Assigned = articleTags.Contains(tag.Id)
                });
            }
        }

    }
}
