using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;

namespace Cust.Pages.Articles
{
    public class IndexModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;

        public IndexModel(Cust.Data.CustContext context)
        {
            _context = context;
        }

        public IList<Article> Article { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Article != null)
            {
                Article = await _context.Article.Include(a => a.Tags).ToListAsync();
            }
        }
    }
}
