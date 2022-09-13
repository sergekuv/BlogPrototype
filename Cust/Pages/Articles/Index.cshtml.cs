using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Cust.Pages.Articles
{
    public class IndexModel : PageModel
    {
        private readonly CustContext _context;
        private readonly IConfiguration Configuration;

        public IndexModel(CustContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        //public IList<Article> Article { get;set; } = default!;
        public PaginatedList<Article> Articles { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            Log.Information("Article Index page OnGetAsync started with sortOrder: {@sortOrder}, currentFilter: {currentFilter}, searchString: {searchString}, pageIndex: {pageIndex}", sortOrder, currentFilter, searchString, pageIndex);
            CurrentSort = sortOrder;
            DateSort = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<Article> articlesIQ = from a in _context.Articles select a;

            //Filter will work properly with case insensitive DB collation
            if (!String.IsNullOrEmpty(searchString))
            {
                articlesIQ = articlesIQ
                    .Include(i => i.Comments)
                    .Include(a => a.Tags).Where(s => s.Title.Contains(searchString)
                                       || s.Content.Contains(searchString)
                                       || s.Tags.Where(t => t.Id.Contains(searchString)).FirstOrDefault() != null);
            }

            switch (sortOrder)
            {
                case "date_asc":
                    articlesIQ = articlesIQ.Include(i => i.Comments).Include(a => a.Tags).OrderBy(r => r.LastEditDate);
                    break;
                default:
                    articlesIQ = articlesIQ.Include(i => i.Comments).Include(a => a.Tags).OrderByDescending(r => r.LastEditDate);
                    break;

            }

            var pageSize = Configuration.GetValue("PageSize", 4);
            Articles = await PaginatedList<Article>.CreateAsync(
                articlesIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            //Articles = await articlesIQ.AsNoTracking().ToListAsync();
        }
    }
}


// Initial version

//namespace Cust.Pages.Articles
//{
//    public class IndexModel : PageModel
//    {
//        private readonly Cust.Data.CustContext _context;

//        public IndexModel(Cust.Data.CustContext context)
//        {
//            _context = context;
//        }

//        public IList<Article> Article { get; set; } = default!;

//        public async Task OnGetAsync()
//        {
//            if (_context.Article != null)
//            {
//                Article = await _context.Article.Include(a => a.Tags).ToListAsync();
//            }
//        }
//    }
//}
