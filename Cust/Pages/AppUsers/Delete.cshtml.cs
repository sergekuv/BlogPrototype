using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;
using Cust.Areas.Identity.Data;

namespace Cust.Pages.AppUsers
{
    public class DeleteModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;

        public DeleteModel(Cust.Data.CustContext context)
        {
            _context = context;
        }
        [BindProperty]
        public CustUser AppUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || _context.CustUser == null)
            {
                return NotFound();
            }

            var user = await _context.CustUser.FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            else
            {
                AppUser = user;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null || _context.CustUser == null)
            {
                return NotFound();
            }
            var user = await _context.CustUser.FindAsync(id);

            if (user != null)
            {
                AppUser = user;
                _context.CustUser.Remove(AppUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

    }
}
