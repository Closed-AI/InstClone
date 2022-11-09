namespace API.Models
{
    public class CreatePostModel
    {
        public string? Description { get; set; }
        public List<MetadataModel>? Contents { get; set; }
    }
}
