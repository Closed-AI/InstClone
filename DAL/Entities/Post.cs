namespace DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset CreatingDate { get; set; }
        public Guid CreatorId { get; set; }

        public virtual User Creator { get; set; } = null!;
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<PostContent>? PostContent { get; set; }
    }
}