using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cust.Data;
using Cust.Models;
using Microsoft.AspNetCore.Authorization;
using Cust.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cust.Pages.Articles
{
    [Authorize]
    public class CreateModel : ArticleTagsPageModel
    {
        private readonly CustContext _context;
        private readonly UserManager<CustUser> _userManager;

        public CreateModel(UserManager<CustUser> userManager, CustContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            Article article = new();
            article.Tags = new List<Tag>();

            PopulateAssignedTagData(_context, article);
            return Page();
        }

        [BindProperty]
        public Article Article { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(string[] selectedTags)
        {

            Article.Author = User.Identity.Name;
            var validationErrors = ModelState.Values.Where(E => E.Errors.Count > 0)
                .SelectMany(E => E.Errors)
                .Select(E => E.ErrorMessage)
                .ToList();

            if (!ModelState.IsValid || _context.Articles == null || Article == null)
            {
                return Page();
            }


            Article newArticle = new();
            newArticle.LastEditDate = DateTime.Now;
            newArticle.Author = User.Identity.Name;

            if (selectedTags.Length > 0)
            {
                newArticle.Tags = new List<Tag>();
                _context.Tags.Load();
            }

            foreach (string tag in selectedTags)
            {
                var foundTag = await _context.Tags.FindAsync(tag);
                if (foundTag != null)
                {
                    newArticle.Tags.Add(foundTag);
                }
            }
            //TODO If some tags are not found, we should add log that.. 

            try
            {
                if(await TryUpdateModelAsync<Article>(
                    newArticle,
                    "article",
                    a => a.Title, a => a.Content))
                {
                    _context.Articles.Add(newArticle);
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                //TODO log error message
            }
            PopulateAssignedTagData(_context, Article);
            return Page();
        }
    }
}
