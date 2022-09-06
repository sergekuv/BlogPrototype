using Cust.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Cust.Models;
using System.Reflection.Emit;

namespace Cust.Data;

public class CustContext : IdentityDbContext<CustUser>
{
    public CustContext(DbContextOptions<CustContext> options)
        : base(options)
    {
    }

    public DbSet<CustUser> CustUser { get; set; }
    public DbSet<Tag> Articles { get; set; }
    public DbSet<Tag> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.Entity<Article>().ToTable(nameof(Article))
            .HasMany(c => c.Tags)
            .WithMany(i => i.Articles);
        builder.Entity<Tag>().ToTable(nameof(Tag));
        builder.Entity<Comment>().ToTable(nameof(Comment));

    }


}
