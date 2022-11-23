namespace API.Models
{
    public class CreateCommentRequest
    {
        public Guid PostId { get; set; }
        public string Text { get; set; } = null!;
    }
}
