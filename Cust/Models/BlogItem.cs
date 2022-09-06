using System.Text;

namespace Cust.Models
{
    public abstract class BlogItem
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public DateTime LastEditDate { get; set; }
        public String Content { get; set; }
    }
}
