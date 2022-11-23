namespace API.Models
{
    public class CreatePostModel
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string? Description { get; set; }
        public List<MetadataLinkModel>? Contents { get; set; }
    }
}
