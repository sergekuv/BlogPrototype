namespace Cust.Models
{
    public class Comment :BlogItem
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
    }
}
