namespace API.Models
{
    public class WriteCommentRequestModel
    {
        public Guid PostId { get; set; }
        public string Text { get; set; } = null!;
    }
}
