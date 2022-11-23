namespace DAL.Entities
{
    public class Subscribe
    {
        public Guid Id { get; set; }

        public Guid TargetId { get; set; }
        public Guid SubscriberId { get; set; }

        public virtual User Target { get; set; } = null!;
        public virtual User Subscriber { get; set; } = null!;
    }
}
