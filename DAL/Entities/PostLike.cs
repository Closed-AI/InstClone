namespace DAL.Entities
{
    public class PostLike : Like
    {
        public Guid PostId { get; set; }

        public virtual Post Comment { get; set; } = null!;
    }
}
