using Cust.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Cust.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cust.Pages.Tags;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CustContextConnection") ?? throw new InvalidOperationException("Connection string 'CustContextConnection' not found.");
//Log.Information("Using ConnectionString {connectionString}", connectionString);

builder.Services.AddDbContext<CustContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//Вопрос для прояснения: стоит ли раскладывать по папкам такой небольшой объем кода? И, если да, то как правильно это делать - так, как сделано ниже, или как-то еще?
//app.MapGet("/Tags", async (CustContext db) =>
//{
//    return   await db.Tags.AsNoTracking().ToListAsync();
//});

app.MapGet("/Tags", async (CustContext db) => await BlogMinApi.Mapped.Tags.Index.TagIndex(db));

app.MapPost("/Tags", async (Tag tag, CustContext db) =>
{
    db.Tags.Add(tag);
    await db.SaveChangesAsync();
    return Results.Created($"/Tags/{tag.Id}", tag);
});

app.MapDelete("/Tags/{id}", async (string id, CustContext db) =>
{
    if (await db.Tags.FindAsync(id) is Tag toDelete)
    {
        db.Tags.Remove(toDelete);
        await db.SaveChangesAsync();
        return Results.Ok(toDelete);
    }

    return Results.NotFound();
});


app.MapGet("/Articles", async (CustContext db) =>
{
    // Наверное, нужно добавить методы для возврата более короткой версии, либо с небольшим кол-вом статей, либо с укороченным содержимым, как это сдклано в Razor-приложении 
    return await db.Articles.Include(a => a.Tags).AsNoTracking().ToListAsync();
});

app.MapGet("/Articles/{id}", async (int id, CustContext db) =>
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
});

app.MapPost("/Articles", async (Article article, CustContext db) =>
{
    // Не лучше ли принимать только название, содержание и тэги? Сделаем так в PUT
    Article newArticle = article;
    newArticle.LastEditDate = DateTime.Now;
    //TODO - .Author needs authentication
    //TODO - check if Author exists and act accordingly

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
    db.Articles.Add(article);
    await db.SaveChangesAsync();

    return Results.Created($"/Articles/{article.Id}", article);
});

app.MapPut("/Articles/{id}", async (int? id, string title, string content, string[]? selectedTags, CustContext db) =>
{
    //Вопрос - что в таких случаях правильнее принимать - целую статью или отдельные поля? 
    //Читается легче статья, чем набор полей...
    if (id == null) return Results.NotFound();

    var articleToUpdate = await db.Articles
        .Include(a => a.Tags)
        .FirstOrDefaultAsync(s => s.Id == id);
    if (articleToUpdate == null) return Results.NotFound();

    articleToUpdate.LastEditDate = DateTime.Now;
    articleToUpdate.Title = title;
    articleToUpdate.Content = content;

    UpdateArticleTags(selectedTags, articleToUpdate);
    await db.SaveChangesAsync(); 
    return Results.NoContent();

    void UpdateArticleTags(string[] selectedTags, Article articleToUpdate)
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
                    var tagToRemove = articleToUpdate.Tags.Single(
                                                    c => c.Id == tag.Id);
                    articleToUpdate.Tags.Remove(tagToRemove);
                }
            }
        }
    }


});


app.MapPost("/Articles/CreateNewComment/{id}/", async (int articleId, string author, string commentContent, CustContext db) =>
{
    //if (articleId == null) return Results.NotFound();
    var articleToUpdate = await db.Articles.FirstOrDefaultAsync(s => s.Id == articleId);
    if (articleToUpdate == null) return Results.NotFound();

    Comment newComment = new();
    newComment.ArticleId = articleId;
    newComment.LastEditDate = DateTime.Now;
    newComment.Author = author;
    newComment.Content = commentContent;

    //TODO - .Author needs authentication
    //TODO - check if Author exists and act accordingly
    db.Comments.Add(newComment);
    await db.SaveChangesAsync();

    return Results.Created($"/Articles/Comments/{articleId}", articleId);
});

app.MapDelete("/Articles/DeleteComment/{id}", async (int id, CustContext db) =>
{
    if (await db.Comments.FindAsync(id) is Comment toDelete)
    {
        db.Comments.Remove(toDelete);
        await db.SaveChangesAsync();
        return Results.Ok(toDelete);
    }

    return Results.NotFound();
});

app.MapDelete("/Articles/{id}", async (int id, CustContext db) =>
{
    if (await db.Articles.FindAsync(id) is Article toDelete)
    {
        db.Articles.Remove(toDelete);
        await db.SaveChangesAsync();
        return Results.Ok(toDelete);
    }

    return Results.NotFound();
});


app.Run();


