using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;

namespace Cust.Pages.Tags
{
    public class IndexModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;

        public IndexModel(Cust.Data.CustContext context)
        {
            _context = context;
        }

        public IList<Tag> Tags { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Articles != null)
            {
                Tags = await _context.Tags.ToListAsync();
            }
        }
    }
}
