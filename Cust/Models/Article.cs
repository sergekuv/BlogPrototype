namespace Cust.Models
{
    public class Article : BlogItem
    {
        public string Title { get; set; }
        public IList<Comment>? Comments { get; set; }
        public ICollection<Tag>? Tags { get; set; }

    }
}
