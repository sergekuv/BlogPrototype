using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trials.Models;

namespace Trials.Data
{
    public class TrialsContext : DbContext
    {
        public TrialsContext (DbContextOptions<TrialsContext> options)
            : base(options)
        {
        }

        public DbSet<Trials.Models.Article> Article { get; set; } = default!;
    }
}
