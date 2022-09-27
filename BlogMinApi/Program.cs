using Cust.Data;
using Microsoft.EntityFrameworkCore;
using Cust.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Cust.Areas.Identity.Data;
using Cust.Authorization;
using BlogMinApi.Mapped;
using BlogMinApi.Dto;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CustContextConnection") ?? throw new InvalidOperationException("Connection string 'CustContextConnection' not found.");

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

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityReq);
});

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
        ValidateLifetime = false, 
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageUsers", policyBuider => policyBuider.AddRequirements(new AllowedManagementRequirement()));
    options.AddPolicy("Editor", policyBuilder => policyBuilder.RequireClaim("IsEditor"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapGet("/Tags", [Authorize] async (CustContext db) => await Tags.TagIndex(db));
app.MapPost("/Tags", [Authorize("Editor")] async (TagDto tag, CustContext db) => await Tags.CreateNewTag(tag, db));
app.MapDelete("/Tags/{id}", [Authorize("Editor")] async ([FromBody] TagDto id, CustContext db) => await Tags.DeleteTag(id, db));

app.MapGet("/Articles", async (CustContext db) => await Articles.ArticleIndex(db));
app.MapGet("/Articles/{id}", async (int id, CustContext db) => await Articles.GetArticle(id, db));
app.MapPost("/Articles", [Authorize] async (ArticleDto article, IHttpContextAccessor httpContextAccessor, CustContext db) => await Articles.CreateNewArticle(article, httpContextAccessor, db));
app.MapPut("/Articles/{id}", [Authorize] async (int id, ArticleDto article, IHttpContextAccessor httpContextAccessor, CustContext db) => await Articles.EditArticle(id, article, httpContextAccessor, db));
app.MapDelete("/Articles/{id}", async (int id, IHttpContextAccessor httpContextAccessor, CustContext db) => await Articles.DeleteArticle(id, httpContextAccessor, db));


app.MapPost("/Articles/CreateNewComment/{id}/", [Authorize] async (int articleId, string author, string commentContent, CustContext db) =>
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

app.MapDelete("/Articles/DeleteComment/{id}", async (int id, IHttpContextAccessor httpContextAccessor, CustContext db) =>
{
    if (await db.Comments.FindAsync(id) is Comment toDelete)
    {
        var userIsAuthor = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value == toDelete.Author;
        var userIsEditor = httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == "IsEditor").FirstOrDefault() != null;
        if (!(userIsAuthor || userIsEditor)) { return Results.Unauthorized(); }

        db.Comments.Remove(toDelete);
        await db.SaveChangesAsync();
        return Results.Ok(toDelete);
    }

    return Results.NotFound();
});



app.MapPost("/minimalapi/security/getToken", [AllowAnonymous] async (UserManager<CustUser> userMgr, UserDto user) =>
{
    var identityUsr = await userMgr.FindByNameAsync(user.UserName);

    if (await userMgr.CheckPasswordAsync(identityUsr, user.Password))
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var securityKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Email, user.UserName),
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
        UserName = user.UserName,
        Alias = user.Alias,
        Email = user.UserName // + "@example.com"
    };

    var result = await userMgr.CreateAsync(identityUser, user.Password);

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






