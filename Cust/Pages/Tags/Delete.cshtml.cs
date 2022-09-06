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
    public class DeleteModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;

        public DeleteModel(Cust.Data.CustContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Tag Tag { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }

            var tag = await _context.Articles.FirstOrDefaultAsync(m => m.Id == id);

            if (tag == null)
            {
                return NotFound();
            }
            else 
            {
                Tag = tag;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }
            var tag = await _context.Articles.FindAsync(id);

            if (tag != null)
            {
                tag.Disabled = true;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
