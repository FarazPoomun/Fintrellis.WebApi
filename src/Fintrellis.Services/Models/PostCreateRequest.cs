namespace Fintrellis.Services.Models
{
    /// <summary>
    /// Request Model for post and put
    /// </summary>
    public class PostCreateRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime PublishedDate { get; set; }
        public string? Author { get; set; }
    }
}
