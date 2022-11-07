namespace API.Models
{
    public class AddAvatarRequestModel
    {
        public MetadataModel Avatar { get; set; } = null!;
        public Guid UserID { get; set; }
    }
}
