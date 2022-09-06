using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trials.Data;
using Trials.Models;

namespace Trials.Pages.Articles
{
    public class IndexModel : PageModel
    {
        private readonly Trials.Data.TrialsContext _context;

        public IndexModel(Trials.Data.TrialsContext context)
        {
            _context = context;
        }

        public IList<Article> Article { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Article != null)
            {
                Article = await _context.Article.ToListAsync();
            }
        }
    }
}
