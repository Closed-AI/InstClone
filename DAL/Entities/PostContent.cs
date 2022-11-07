namespace DAL.Entities
{
    public class PostContent : Attach
    {
        public Guid PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
    }
}