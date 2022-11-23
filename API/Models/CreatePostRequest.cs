namespace API.Models
{
    public class CreatePostRequest
    {
        public Guid? AuthorId { get; set; }
        public string? Description { get; set; }
        public List<MetadataModel>? Contents { get; set; } = new List<MetadataModel>();
    }
}
