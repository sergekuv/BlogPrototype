using Cust.Models;

namespace BlogWebApi.Models
{
    public class ArticleEdit    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ICollection<Tag>? Tags { get; set; }
    }
}
