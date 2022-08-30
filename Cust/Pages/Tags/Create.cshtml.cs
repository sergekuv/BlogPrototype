using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cust.Data;
using Cust.Models;

namespace Cust.Pages.Tags
{
    public class CreateModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;

        public CreateModel(Cust.Data.CustContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (_context.Tag.Where(t => t.Id.ToUpper() == Tag.Id.ToUpper()).FirstOrDefault() != null)
            {
                ModelState.AddModelError(String.Empty, "Tag with this name already exists");
            }

          if (!ModelState.IsValid || _context.Tag == null || Tag == null)
            {
                return Page();
            }

            _context.Tag.Add(Tag);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
