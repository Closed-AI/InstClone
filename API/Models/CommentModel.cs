namespace API.Models
{
    public class CommentModel
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }

        public string Text { get; set; } = null!;
        public DateTimeOffset CreatingDate { get; set; }

        public int LikeCount { get; set; }
    }
}
