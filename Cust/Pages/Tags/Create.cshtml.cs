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
using Serilog;

namespace Cust.Pages.Tags
{
    [Authorize("Editor")]
    public class CreateModel : PageModel
    {
        private readonly Cust.Data.CustContext _context;

        public CreateModel(Cust.Data.CustContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            Log.Information("Pages -> Tags -> Create -> OnGet started");
            return Page();
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            string logPrefix = "Pages -> Tags -> Create -> OnPostAsync: ";
            Log.Information(logPrefix + "started");
            var validationErrors = ModelState.Values.Where(E => E.Errors.Count > 0)
                .SelectMany(E => E.Errors)
                .Select(E => E.ErrorMessage)
            .ToList();
            if (validationErrors.Any())
            {
                Log.Warning(logPrefix + "The followiing Validation errors found when adding Tag:");
                foreach (var error in validationErrors)
                {
                    Log.Warning(error);
                }
            }

            bool tagAlreadyExists = _context.Tags.Where(t => t.Id.ToUpper() == Tag.Id.ToUpper()).Any();
            if (tagAlreadyExists)
            {
                ModelState.AddModelError(string.Empty, "Tag already exists: " + Tag.Id);
                Log.Warning(logPrefix +" trying to add duplicate tag: {tagId}", Tag.Id);
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
                Log.Information(logPrefix + "awaiting for SaveChangesAsync for tag {tag}", Tag.Id);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}
