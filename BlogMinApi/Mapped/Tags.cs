using BlogMinApi.Dto;
using Cust.Data;
using Cust.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogMinApi.Mapped;

public static class Tags
{
    public static async Task<List<Tag>> TagIndex(CustContext db)
    {
        return await db.Tags.AsNoTracking().ToListAsync();
    }

    public static async Task<IResult> CreateNewTag(TagDto tag, CustContext db)
    {
        Tag newTag = new();
        newTag.Id = tag.Id;
        newTag.Disabled = false;
        db.Tags.Add(newTag);
        await db.SaveChangesAsync();
        return Results.Created($"/Tags/{newTag.Id}", newTag);
    }

    public static async Task<IResult> DeleteTag(TagDto tagToDelete, CustContext db)
    {
        if (await db.Tags.FindAsync(tagToDelete.Id) is Tag toDelete)
        {
            db.Tags.Remove(toDelete);
            await db.SaveChangesAsync();
            return Results.Ok(toDelete);
        }
        return Results.NotFound();
    }
}
