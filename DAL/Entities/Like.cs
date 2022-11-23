namespace DAL.Entities
{
    public class Like
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
