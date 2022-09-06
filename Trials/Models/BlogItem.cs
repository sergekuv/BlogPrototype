using System.Text;

namespace Trials.Models
{
    public abstract class BlogItem
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public DateTime LastEditDate { get; set; }
        public String Content { get; set; }
    }
}
