namespace Cust.Models.BlogViewModels
{
    public class ArticleCommentsData
    {
        public Article Article { get; set; }
        public IEnumerable<Comment>? Comments { get; set; }

    }
}
