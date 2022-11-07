namespace API.Models
{
    public class AddContentRequestModel
    {
        public Guid PostId { get; set; }
        public MetadataModel Meta { get; set; } = null!;
    }
}
