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
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    #endregion
    {
        private readonly CustContext _context;

        public TagsController(CustContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            return await _context.Tags.ToListAsync();
        }

        // GET: api/Tags/5
        #region snippet_GetByID
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(string id)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }
        #endregion

        // PUT: api/Tags/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        #region snippet_Update
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(string id, Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest();
            }

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        // POST: api/Tags
        // Нужно бы еще оверпостинг доработать..https://go.microsoft.com/fwlink/?linkid=2123754
        #region snippet_Create
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTag", new { id = tag.Id }, tag);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }
        #endregion

        // DELETE: api/Tags/5
        #region snippet_Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(string id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool TagExists(string id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
