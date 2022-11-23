namespace API.Models
{
    public class CreateCommentModel
    {
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; } = null!;
    }
}
