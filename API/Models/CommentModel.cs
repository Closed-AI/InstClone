namespace API.Models
{
    public class CommentModel
    {
        public string Text { get; set; } = null!;
        public DateTimeOffset CreatingDate { get; set; }
    }
}
