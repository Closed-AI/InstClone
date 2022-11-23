namespace DAL.Entities
{
    public class CommentLike : Like
    {
        public Guid CommentId { get; set; }

        public virtual Comment Comment { get; set; } = null!;
    }
}
