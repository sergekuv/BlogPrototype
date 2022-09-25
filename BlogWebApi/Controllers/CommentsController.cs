using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cust.Models;
using Cust.Data;

namespace BlogWebApi.Controllers
{
    #region snippet_Route
    [Route("api/Articles/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    #endregion
    {
        private readonly CustContext _context;

        public CommentsController(CustContext context)
        {
            _context = context;
        }


        //// GET: api/Comments
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        //{
        //    return await _context.Comments.ToListAsync();
        //}

        // GET: api/Comments/5
        #region snippet_GetByID
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }
        #endregion

        // PUT: api/Articles/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        #region snippet_Update
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        #endregion

        #region snippet_Create
        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(int articleId, string author, string commentContent)
        {
            var articleToUpdate = await _context.Articles.FirstOrDefaultAsync(s => s.Id == articleId);
            if (articleToUpdate == null) return NotFound();

            Comment newComment = new();
            newComment.ArticleId = articleId;
            newComment.LastEditDate = DateTime.Now;
            newComment.Author = author;
            newComment.Content = commentContent;

            //TODO - .Author needs authentication
            //TODO - check if Author exists and act accordingly
            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return Created($"/Articles/Comments/{newComment.Id}", newComment.Id);
        }
        #endregion

        // DELETE: api/Comments/5
        #region snippet_Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(c => c.Id == id);
        }
    }
}
