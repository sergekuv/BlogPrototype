using Cust.Data;
using Cust.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogMinApi.Mapped.Tags
{
    public class Index
    {
        public static async Task<List<Tag>> TagIndex(CustContext db)
        {
            return await db.Tags.AsNoTracking().ToListAsync(); 
        }
    }
}
