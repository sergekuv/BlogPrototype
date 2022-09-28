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
using Microsoft.AspNetCore.Http;

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

app.MapPost("/Articles/CreateNewComment/{id}/", [Authorize] async (CommentDto comment, IHttpContextAccessor httpContextAccessor, CustContext db) => await Comments.CreateNewComment(comment, httpContextAccessor, db));
app.MapDelete("/Articles/DeleteComment/{id}", async (int id, IHttpContextAccessor httpContextAccessor, CustContext db) => await Comments.DeleteComment(id, httpContextAccessor, db));


app.MapPost("/minimalapi/security/getToken", [AllowAnonymous] async (UserManager<CustUser> userMgr, UserDto user) => await Users.GetToken(builder, userMgr, user));
app.MapPost("/minimalapi/security/createUser", [AllowAnonymous] async (UserManager<CustUser> userMgr, UserDto user) => await Users.CreateUser(userMgr, user));

app.Run();






