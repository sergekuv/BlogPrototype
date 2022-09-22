using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Models;
using Cust.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Cust.Pages.Articles
{
    public class EditModel : ArticleTagsPageModel
    {
        private readonly Cust.Data.CustContext _context;
        private readonly UserManager<CustUser> _userManager;

        public EditModel(UserManager<CustUser> userManager, CustContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Article Article { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Article == null)
            {
                return NotFound();
            }

            var article =  await _context.Article
                .Include(a => a.Tags)
               // .Include(a => a.Comments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }
            Article = article;
            
            PopulateAssignedTagData(_context, Article);
            return Page();
        }

        // TODO modify to prevent overposting https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(int? id, string[] selectedTags)
        {
            if (id == null)
            {
                return NotFound();
            }
            var articleToUpdate = await _context.Articles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (articleToUpdate == null)
            {
                return NotFound();
            }

            articleToUpdate.LastEditDate = DateTime.Now;
            if (await TryUpdateModelAsync<Article>(
                articleToUpdate,
                "Article",
                a => a.Title, a => a.Content))
            {
                UpdateArticleTags(selectedTags, articleToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            UpdateArticleTags(selectedTags, articleToUpdate);
            PopulateAssignedTagData(_context, articleToUpdate);
            return Page();

        }


        private bool ArticleExists(int id)
        {
          return (_context.Article?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public void UpdateArticleTags(string[] selectedTags,Article articleToUpdate)
        {
            if (selectedTags == null)
            {
                articleToUpdate.Tags = new List<Tag>();
                return;
            }

            var selectedTagsHS = new HashSet<string>(selectedTags);
            var articleTags = new HashSet<string> (articleToUpdate.Tags.Select(t => t.Id));

            foreach (var tag in _context.Tags)
            {
                if (selectedTagsHS.Contains(tag.Id))
                {
                    if (!articleTags.Contains(tag.Id))
                    {
                        articleToUpdate.Tags.Add(tag);
                    }
                }
                else
                {
                    if (articleTags.Contains(tag.Id))
                    {
                        var tagToRemove = articleToUpdate.Tags.Single(
                                                        c => c.Id == tag.Id);
                        articleToUpdate.Tags.Remove(tagToRemove);
                    }
                }
            }
        }
    }
}
