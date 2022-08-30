using Cust.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Cust.Models;

namespace Cust.Data;

public class CustContext : IdentityDbContext<CustUser>
{
    public CustContext(DbContextOptions<CustContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<Cust.Models.Student>? Student { get; set; }
    public DbSet<Cust.Areas.Identity.Data.CustUser>? CustUser { get; set; }

}
