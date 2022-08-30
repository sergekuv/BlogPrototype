using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Cust.Data;
using Cust.Areas.Identity.Data;
using Cust.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CustContextConnection") ?? throw new InvalidOperationException("Connection string 'CustContextConnection' not found.");

builder.Services.AddDbContext<CustContext>(options =>
    options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<CustUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<CustContext>();

builder.Services.AddDefaultIdentity<CustUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.AllowedForNewUsers = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

}).AddEntityFrameworkStores<CustContext>();



// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageUsers", policyBuider => policyBuider.AddRequirements(new AllowedManagementRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, UserAdminHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();;

app.UseAuthorization();

app.MapRazorPages();

app.Run();
