using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cust.Data;
using Cust.Models;

namespace Cust.Pages.Articles
{
    public class CreateNewCommentModel : PageModel
    {
        private readonly CustContext _context;
        Comment _comment { get; set; }

        public CreateNewCommentModel(CustContext context)
        {
            _context = context;
            _comment = new Comment();
        }

        public IActionResult OnGet(int articleId)
        {
            //_comment.ArticleId = articleId;
            Comment = new();
            Comment.ArticleId = articleId;
            Comment.LastEditDate = DateTime.Now;
            Comment.Author = User.Identity.Name;
            //ViewData["ArticleId"] = new SelectList(_context.Articles, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Comment Comment { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
         var validationErrors = ModelState.Values.Where(E => E.Errors.Count > 0)
            .SelectMany(E => E.Errors)
            .Select(E => E.ErrorMessage)
            .ToList();


            if (!ModelState.IsValid || _context.Comments == null || Comment == null)
            {

                return Page();
            }

            Comment.Author = User.Identity.Name;
            Comment.LastEditDate = DateTime.Now;
            //Comment.ArticleId = _comment.ArticleId;

            _context.Comments.Add(Comment);
            await _context.SaveChangesAsync();

            //return RedirectToPage("./Details");
            return RedirectToPage("./Details", new {id=Comment.ArticleId});

        }
    }
}
