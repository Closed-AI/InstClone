namespace API.Models
{
    public class CommentModel
    {
        public Guid AuthorId { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset CreatingDate { get; set; }
    }
}
