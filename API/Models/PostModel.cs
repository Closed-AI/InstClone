namespace API.Models
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public UserWithAvatarModel Author { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset CreatingDate { get; set; }
        public List<AttachWithLinkModel>? Contents { get; set; }
        
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
    }
}
