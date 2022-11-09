namespace API.Models
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public Guid CreatorId { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset CreatingDate { get; set; }
        public List<AttachWithLinkModel>? Contents { get; set; }
        public List<CommentModel>? Comments { get; set; }
    }
}
