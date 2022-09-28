using BlogMinApi.Dto;
using Cust.Data;
using Cust.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogMinApi.Mapped;

public static class Articles
{
    public static async Task<List<Article>> ArticleIndex(CustContext db)
    {
        // Наверное, нужно добавить методы для возврата более короткой версии, либо с небольшим кол-вом статей,
        // либо с укороченным содержимым, как это сдклано в Razor-приложении 
        return await db.Articles
            .Include(a => a.Tags)
            .AsNoTracking()
            .ToListAsync();
    }

    public static async Task<IResult> GetArticle(int id, CustContext db)
    {
        var article = await db.Articles.Where(a => a.Id == id)
        .Include(a => a.Tags)
        .Include(a => a.Comments)
        .FirstOrDefaultAsync();

        if (article != null)
        {
            return Results.Ok(article);
        }
        return Results.NotFound();
    }


    public static async Task<IResult> CreateNewArticle(ArticleDto article, IHttpContextAccessor httpContextAccessor, CustContext db)
    {
        Article newArticle = new();
        newArticle.Title = article.Title;
        newArticle.Content = article.Content;
        newArticle.LastEditDate = DateTime.Now;
        newArticle.Author = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

        //if (article.Tags != null && article.Tags.Count() > 0)
        if (article.Tags?.Count() > 0)
        {
            newArticle.Tags = new List<Tag>();
            db.Tags.Load();
        }

        foreach (var tag in article.Tags)
        {
            var foundTag = await db.Tags.FindAsync(tag);
            if (foundTag != null)
            {
                newArticle.Tags.Add(foundTag);
            }
        }
        db.Articles.Add(newArticle);
        await db.SaveChangesAsync();

        return Results.Created($"/Articles/{newArticle.Id}", newArticle);
    }

    public static async Task<IResult> EditArticle(int id, ArticleDto article, IHttpContextAccessor httpContextAccessor, CustContext db)
    {
        var articleToUpdate = await db.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (articleToUpdate == null) return Results.NotFound();
        if (!IsAuthorOrEditor(articleToUpdate, httpContextAccessor)) { return Results.Unauthorized(); }

        articleToUpdate.LastEditDate = DateTime.Now;
        articleToUpdate.Title = article.Title;
        articleToUpdate.Content = article.Content;

        UpdateArticleTags(article.Tags, articleToUpdate);
        await db.SaveChangesAsync();
        return Results.NoContent();

        void UpdateArticleTags(ICollection<string> selectedTags, Article articleToUpdate)
        {
            if (selectedTags == null)
            {
                articleToUpdate.Tags = new List<Tag>();
                return;
            }

            var selectedTagsHS = new HashSet<string>(selectedTags);
            var articleTags = new HashSet<string>(articleToUpdate.Tags.Select(t => t.Id));

            foreach (var tag in db.Tags)
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
                        var tagToRemove = articleToUpdate.Tags.Single(c => c.Id == tag.Id);
                        articleToUpdate.Tags.Remove(tagToRemove);
                    }
                }
            }
        }
    }

    public static async Task<IResult> DeleteArticle(int id, IHttpContextAccessor httpContextAccessor, CustContext db)
    {
        if (await db.Articles.FindAsync(id) is Article toDelete)
        {
            if (!IsAuthorOrEditor(toDelete, httpContextAccessor)) { return Results.Unauthorized(); }

            db.Articles.Remove(toDelete);
            await db.SaveChangesAsync();
            return Results.Ok(toDelete);
        }
        return Results.NotFound();
    }


    private static bool IsAuthorOrEditor (BlogItem article, IHttpContextAccessor httpContextAccessor)
    {
        var isAuthor = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value == article.Author;
        var isEditor = httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == "IsEditor").FirstOrDefault() != null;
        return isAuthor || isEditor;
    }
}