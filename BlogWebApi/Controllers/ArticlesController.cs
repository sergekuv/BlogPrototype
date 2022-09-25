using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cust.Models;
using Cust.Data;
using System.Reflection.Metadata;
using BlogWebApi.Models;

namespace BlogWebApi.Controllers
{
    #region snippet_Route
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    #endregion
    {
        private readonly CustContext _context;

        public ArticlesController(CustContext context)
        {
            _context = context;
        }

        // GET: api/Articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetTags()
        {
            return await _context.Articles.Include(a => a.Tags).ToListAsync();
        }

        // GET: api/Articles/5
        #region snippet_GetByID
        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await _context.Articles
                .Where(a => a.Id == id)
                .Include(a => a.Tags)
                .Include(ar => ar.Comments)
                .FirstOrDefaultAsync();

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }
        #endregion

        // PUT: api/Articles/5
        #region snippet_Update
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int? id, string title, string content, string[]? selectedTags)
        {
            if (id == null) return BadRequest();
            var articleToUpdate = await _context.Articles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (articleToUpdate == null) return NotFound();

            articleToUpdate.LastEditDate = DateTime.Now;
            articleToUpdate.Title = title;
            articleToUpdate.Content = content;

            UpdateArticleTags(selectedTags, articleToUpdate);
            await _context.SaveChangesAsync();
            return NoContent();

            void UpdateArticleTags(string[] selectedTags, Article articleToUpdate)
            {
                if (selectedTags == null)
                {
                    articleToUpdate.Tags = new List<Tag>();
                    return;
                }

                var selectedTagsHS = new HashSet<string>(selectedTags);
                var articleTags = new HashSet<string>(articleToUpdate.Tags.Select(t => t.Id));

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

        #endregion

        // POST: api/Articles
        // Нужно бы еще оверпостинг доработать..https://go.microsoft.com/fwlink/?linkid=2123754
        #region snippet_Create
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(ArticleEdit article)
        {
            Article newArticle = new();
            newArticle.Author = article.Author; //..нужно бы сделать аутентификацию..
            newArticle.Title = article.Title;
            newArticle.Content = article.Content;
            newArticle.LastEditDate = DateTime.Now;
            //TODO - .Author needs authentication-authorization
            //TODO - check if Author exists and act accordingly

            if (article.Tags?.Count() > 0)
            {
                newArticle.Tags = new List<Tag>();
                _context.Tags.Load();
            }

            foreach (var tag in article.Tags)
            {
                var foundTag = await _context.Tags.Where(t => t.Id== tag.Id).FirstOrDefaultAsync(); // .FindAsync(tag);
                if (foundTag != null)
                {
                    newArticle.Tags.Add(foundTag);
                }
            }
            _context.Articles.Add(newArticle);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTag", new { id = tag.Id }, tag);
            return CreatedAtAction(nameof(GetArticle), new { id = newArticle.Id }, newArticle);
        }
        #endregion
        #region snippet_CreateComment
        //[HttpPost]
        //public async Task<ActionResult<Article>> PostArticleCreateNewComment(int articleId, string author, string commentContent, CustContext db)
        //{
        //    var articleToUpdate = await db.Articles.FirstOrDefaultAsync(s => s.Id == articleId);
        //    if (articleToUpdate == null) return NotFound();

        //    Comment newComment = new();
        //    newComment.ArticleId = articleId;
        //    newComment.LastEditDate = DateTime.Now;
        //    //TODO - .Author needs authentication
        //    //TODO - check if Author exists and act accordingly
        //    newComment.Author = author;
        //    newComment.Content = commentContent;

        //    db.Comments.Add(newComment);
        //    await db.SaveChangesAsync();

        //    return Created($"/Articles/Comments/{articleId}", articleId);
        //}
        #endregion


        // DELETE: api/Tags/5
        #region snippet_Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(a => a.Id == id);
        }
    }
}
