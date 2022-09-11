using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;
using Cust.Models.BlogViewModels;

namespace Cust.Pages.Articles
{
    public class DetailsModel : PageModel
    {
        private readonly CustContext _context;

        public DetailsModel(CustContext context)
        {
            _context = context;
        }

        //public ArticleCommentsData ArticleComments { get; set; }


        public Article Article { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Article == null)
            {
                return NotFound();
            }

            var article = await _context.Article.Include(a => a.Comments).FirstOrDefaultAsync(m => m.Id == id) ;
            if (article == null)
            {
                return NotFound();
            }

            Article = article;
            return Page();
        }
    }
}



// ---- Initial Version --------
//namespace Cust.Pages.Articles
//{
//    public class DetailsModel : PageModel
//    {
//        private readonly Cust.Data.CustContext _context;

//        public DetailsModel(Cust.Data.CustContext context)
//        {
//            _context = context;
//        }

//        public Article Article { get; set; } = default!;

//        public async Task<IActionResult> OnGetAsync(int? id)
//        {
//            if (id == null || _context.Article == null)
//            {
//                return NotFound();
//            }

//            var article = await _context.Article.FirstOrDefaultAsync(m => m.Id == id);
//            if (article == null)
//            {
//                return NotFound();
//            }
//            else
//            {
//                Article = article;
//            }
//            return Page();
//        }
//    }
//}
