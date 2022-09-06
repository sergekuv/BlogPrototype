using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cust.Data;
using Cust.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Cust.Pages.Tags
{
    [Authorize("TagEditor")]
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
            var validationErrors = ModelState.Values.Where(E => E.Errors.Count > 0)
                .SelectMany(E => E.Errors)
                .Select(E => E.ErrorMessage)
            .ToList();

            bool tagAlreadyExists = _context.Tags.Where(t => t.Id.ToUpper() == Tag.Id.ToUpper()).Any();
            if (tagAlreadyExists)
            {
                ModelState.AddModelError(string.Empty, "Tag already exists");
            }

            if (!ModelState.IsValid || _context.Tags == null || Tag == null)
            {
                return Page();
            }

            Tag emptyTag = new();

            if (await TryUpdateModelAsync<Tag>(
                emptyTag,
                "tag",
                t => t.Id, t => t.Disabled))
            {
                _context.Tags.Add(emptyTag);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }

            return Page();
            //if (!ModelState.IsValid || _context.Articles == null || Tag == null)
            //{
            //    return Page();
            //}

            //_context.Articles.Add(Tag);
            //await _context.SaveChangesAsync();

            //return RedirectToPage("./Index");
        }
    }
}
