using Cust.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Cust.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cust.Pages.Tags;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Cust.Areas.Identity.Data;
using Cust.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CustContextConnection") ?? throw new InvalidOperationException("Connection string 'CustContextConnection' not found.");
//Log.Information("Using ConnectionString {connectionString}", connectionString);

builder.Services.AddDbContext<CustContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

var contactInfo = new OpenApiContact()
{
    Name = "Test",
    Email = "test.com",
    Url = new Uri("https://test.com")
};

var license = new OpenApiLicense()
{
    Name = "Free License",
};

var info = new OpenApiInfo()
{
    Version = "V1",
    Title = "Blog Api with JWT Authentication",
    Description = "Blog Api with JWT Authentication",
    Contact = contactInfo,
    License = license
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityReq);
});

//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//                .AddEntityFrameworkStores<CustContext>()
//                .AddDefaultTokenProviders();

builder.Services.AddDefaultIdentity<CustUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.AllowedForNewUsers = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

})
    .AddEntityFrameworkStores<CustContext>()
    .AddDefaultTokenProviders();




builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = false, // In any other application other then demo this needs to be true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthentication();
//builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageUsers", policyBuider => policyBuider.AddRequirements(new AllowedManagementRequirement()));
    ////options.AddPolicy("TagEditor", policyBuilder => policyBuilder.RequireClaim("IsAdmin"));
   // options.AddPolicy("Editor", policyBuilder => policyBuilder.RequireClaim("IsEditor"));
    options.AddPolicy("Editor", policyBuilder => policyBuilder.RequireClaim("IsEditor"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

//Вопрос для прояснения: стоит ли раскладывать по папкам такой небольшой объем кода? И, если да, то как правильно это делать - так, как сделано ниже, или как-то еще?
//app.MapGet("/Tags", async (CustContext db) =>
//{
//    return   await db.Tags.AsNoTracking().ToListAsync();
//});

app.MapGet("/Tags", [Authorize] async (CustContext db) => await BlogMinApi.Mapped.Tags.Index.TagIndex(db));

app.MapPost("/Tags", [Authorize("Editor")] async (Tag tag, CustContext db) =>
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
}).RequireAuthorization("Editor"); ;

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

#region AuthWithConstUser
//app.MapPost("/accounts/login", [AllowAnonymous] (UserDto user) => {
//    if (user.username == "1@1.1" && user.password == "1")
//    {
//        var secureKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

//        var issuer = builder.Configuration["Jwt:Issuer"];
//        var audience = builder.Configuration["Jwt:Audience"];
//        var securityKey = new SymmetricSecurityKey(secureKey);
//        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

//        var jwtTokenHandler = new JwtSecurityTokenHandler();

//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(new[] {
//                new Claim("Id", "1"),
//                new Claim(JwtRegisteredClaimNames.Sub, user.username),
//                new Claim(JwtRegisteredClaimNames.Email, user.username),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//            }),
//            Expires = DateTime.Now.AddMinutes(5),
//            Audience = audience,
//            Issuer = issuer,
//            SigningCredentials = credentials
//        };

//        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
//        var jwtToken = jwtTokenHandler.WriteToken(token);
//        return Results.Ok(jwtToken);
//    }
//    return Results.Unauthorized();
//});
#endregion

app.MapPost("/minimalapi/security/getToken",
[AllowAnonymous]
async (UserManager<CustUser> userMgr, UserDto user) =>
{
//    var identityUsr = await userMgr.FindByNameAsync(user.username);

//    if (await userMgr.CheckPasswordAsync(identityUsr, user.password))
//    {
//        var issuer = builder.Configuration["Jwt:Issuer"];
//        var audience = builder.Configuration["Jwt:Audience"];
//        var securityKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
//        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
//        var token = new JwtSecurityToken(issuer: issuer,
//            audience: audience,
//            signingCredentials: credentials);
//        var tokenHandler = new JwtSecurityTokenHandler();
//        var stringToken = tokenHandler.WriteToken(token);
//        return Results.Ok(stringToken);
//    }
//    else
//    {
//        return Results.Unauthorized();
//    }
//});

    var identityUsr = await userMgr.FindByNameAsync(user.username);

    if (await userMgr.CheckPasswordAsync(identityUsr, user.password))
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var securityKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                        //new Claim("IsEditor",""),
                        new Claim(JwtRegisteredClaimNames.Sub, user.username),
                        new Claim(JwtRegisteredClaimNames.Email, user.username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
           // Expires = DateTime.Now.AddMinutes(5),
            Audience = audience,
            Issuer = issuer,
            SigningCredentials = credentials
        };

        var adminClaim = userMgr.GetClaimsAsync(identityUsr).Result.Where(c => c.Type == "IsAdmin").FirstOrDefault();
        if (adminClaim != null)
        {
            tokenDescriptor.Subject.Claims.Append(new Claim("IsAdmin", ""));
        }

        var editorClaim = userMgr.GetClaimsAsync(identityUsr).Result.Where(c => c.Type == "IsEditor").FirstOrDefault();
        if (editorClaim != null)
        {
            tokenDescriptor.Subject.AddClaim(new Claim("IsEditor", ""));
        }

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);
      
        //var token = new JwtSecurityToken(issuer: issuer, audience: audience,signingCredentials: credentials);
        //var tokenHandler = new JwtSecurityTokenHandler();
        //var stringToken = tokenHandler.WriteToken(token);
        var stringToken = jwtTokenHandler.WriteToken(token);

        return Results.Ok(stringToken);
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/minimalapi/security/createUser", [AllowAnonymous] async (UserManager<CustUser> userMgr, UserDto user) =>
{
    var identityUser = new CustUser()
    {
        UserName = user.username,
        Alias = user.alias,
        Email = user.username // + "@example.com"
    };

    var result = await userMgr.CreateAsync(identityUser, user.password);

    if (result.Succeeded)
    {
        return Results.Ok();
    }
    else
    {
        return Results.BadRequest();
    }
});

app.Run();
record UserDto(string username, string alias, string password);



