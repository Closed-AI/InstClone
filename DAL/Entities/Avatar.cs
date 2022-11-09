namespace DAL.Entities
{
    public class Avatar : Attach
    {
        public Guid UserID { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
