namespace API.Models
{
    public class AttachWithLinkModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string? Link { get; set; } = null!;
    }
}
